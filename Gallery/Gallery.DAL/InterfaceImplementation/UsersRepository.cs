﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gallery.DAL.Interfaces;
using Gallery.DAL.Models;

namespace Gallery.DAL.InterfaceImplementation
{
    public class UsersRepository : IRepository
    {
        private readonly UserContext _context = new UserContext();

        public UsersRepository(UserContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }

        public async Task<bool> IsUserExistAsync(string username, string plainPassword)
        {

            return await _context.Users.AnyAsync(u => u.Email == username.Trim().ToLower() && u.Password == plainPassword.Trim());

        }

        public async Task AddUserToDatabaseAsync(string username, string plainPassword, int roleId)
        {
            _context.Users.Add(new User { Email = username, Password = plainPassword, RoleId = roleId});
            _context.SaveChanges();
        }

        public int GetIdUsers(string username)
        {
            return _context.Users.Where(u => u.Email == username).Select(u => u.Id).FirstOrDefault();
        }

        public string GetNameUsers(int id)
        {
            return _context.Users.Where(u => u.Id == id).Select(u => u.Email).FirstOrDefault();
        }

    }
}
