﻿using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Gallery.BLL.Contract;
using Gallery.BLL.Interfaces;
using Gallery.DAL;
using Gallery.DAL.Models;
using Gallery.Filters;
using Gallery.Models.AccountModels;

namespace Gallery.Controllers
{
    public class AccountController : Controller
    {

        private readonly IUsersService _usersService;
        private readonly IAuthenticationService _authenticationService;

        public AccountController(IUsersService usersService, IAuthenticationService authenticationService)
        {
            _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }


        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModelState]
        [LogFilter]
        public async Task<ActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {

                var isUserExist = await _usersService.IsUserExistByEmailAsync(new UserDto
                {
                    UserEmail = model.Email,
                    UserPassword = model.Password
                });

                if (isUserExist)
                {
                    var canAuthorize = await _usersService.IsUserExistAsync(new UserDto
                    {
                        UserEmail = model.Email,
                        UserPassword = model.Password
                    });
                    var ipAddress = HttpContext.Request.UserHostAddress;
                    AttemptDTO attemptDto = new AttemptDTO
                    {
                        Email = model.Email,
                        IpAddress = ipAddress,
                        IsSuccess = canAuthorize
                    };

                    await _usersService.AddAttemptAsync(new AttemptDTO
                    {
                        Email = model.Email,
                        IpAddress = ipAddress,
                        IsSuccess = canAuthorize
                    });


                    if (canAuthorize)
                    {
                        var userDto = await _usersService.FindUserAsync(new UserDto
                        {
                            UserEmail = model.Email,
                            UserPassword = model.Password
                        });


                        var claim = _authenticationService.ClaimTypesСreation(userDto);
                        _authenticationService.OwinCookieAuthentication(HttpContext.GetOwinContext(), claim);

                        return RedirectToAction("Index", "Home");

                    }
                    else
                    {
                        ModelState.AddModelError("", "User is not found");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User not found");
                }
            }

            return View(model);
        }

        public ActionResult LoginOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModelState]
        public async Task<ActionResult> Register(RegisterModel model)
        {

            if (ModelState.IsValid)
            {
               
                var IfUserExist = await _usersService.IsUserExistAsync(new UserDto
                {
                    UserEmail = model.Email,
                    UserPassword = model.Password
                });
                var ipAddress = HttpContext.Request.UserHostAddress;
                
                

                if (IfUserExist == false)
                {
                    //Create a new user
                    await _usersService.AddUserAsync(new UserDto
                    {
                        UserEmail = model.Email,
                        UserPassword = model.Password
                    });

                    await _usersService.AddAttemptAsync(new AttemptDTO
                    {
                        Email = model.Email,
                        IpAddress = ipAddress,
                        IsSuccess = true
                    });

                    var userDto = await _usersService.FindUserAsync(new UserDto
                    {
                        UserEmail = model.Email,
                        UserPassword = model.Password
                    });

                    var claim = _authenticationService.ClaimTypesСreation(userDto);
                    _authenticationService.OwinCookieAuthentication(HttpContext.GetOwinContext(), claim);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "User already exists");
                }
            }

            return View(model);
        }
    }
}