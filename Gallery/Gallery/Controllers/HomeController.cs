﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Gallery.ConfigManagement;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Gallery.BLL.Interfaces;
using Gallery.BLL.Services;

namespace Gallery.Controllers
{
    public class HomeController : Controller
    {

        private IHashService _hashService;
        private IImagesService _imagesService;
        private readonly ConfigurationManagement Config = new ConfigurationManagement();

        public HomeController(IImagesService imageService, IHashService hashService)
        {
            _imagesService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        }
       


        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult Delete(string PathFileDelete = "")
        {
            try
            {

                if (PathFileDelete.Replace(Config.СheckValuePathToPhotos(), "").Replace(Path.GetFileName(PathFileDelete), "").Replace("/", "") == _hashService.ComputeSha256Hash("Dima"))
                {
                    if (PathFileDelete != "" && Directory.Exists(Server.MapPath(PathFileDelete.Replace(Path.GetFileName(PathFileDelete), ""))))
                        System.IO.File.Delete(Server.MapPath(PathFileDelete));
                    else
                    {
                        ViewBag.Error = "File not found!";
                        return View("Error");
                    }
                }
                else
                {
                    ViewBag.Error = "Authorisation Error!";
                    return View("Error");
                }
            }
            catch (Exception err)
            {
                ViewBag.Error = "Unexpected error: " + err.Message;
                return View("Error");
            }
            return RedirectToAction("Index");
        }



        [HttpPost]
        [Authorize]
        public ActionResult Upload(HttpPostedFileBase files)
        {
            try
            {
                if (files != null)
                {
                    if (Config.СheckValueFileExtensions().Contains(files.ContentType))
                    {
                        FileStream TempFileStream;
                        // Verify that the user selected a file and User is logged in
                        if (files.ContentLength > 0)
                        {
                            bool IsLoad = true;
                            // Encrypted User's directory path
                            string DirPath = Server.MapPath(Config.СheckValuePathToPhotos()) + _hashService.ComputeSha256Hash("Dima");

                            // extract only the filename
                            var fileName = Path.GetFileName(files.FileName);
                            // store the file inside ~/Content/Temp folder
                            var TempPath = Path.Combine(Server.MapPath("~/Content/Temp"), fileName);
                            files.SaveAs(TempPath);
                            TempFileStream = new FileStream(TempPath, FileMode.Open);
                            BitmapSource bitmapSource = BitmapFrame.Create(TempFileStream);
                            BitmapMetadata bitmapMetadata = (BitmapMetadata)bitmapSource.Metadata;
                            var DateTaken = bitmapMetadata.DateTaken;
                            TempFileStream.Close();

                            if (!string.IsNullOrEmpty(DateTaken) || files.ContentType != "image/jpeg")
                            {
                                if (Convert.ToDateTime(DateTaken) >= DateTime.Now.AddYears(-1) || files.ContentType != "image/jpeg")
                                {
                                    TempFileStream = new FileStream(TempPath, FileMode.Open);
                                    Bitmap TempBmp = new Bitmap(TempFileStream);
                                    TempBmp = new Bitmap(TempBmp, 64, 64);
                                    TempFileStream.Close();

                                    // List of all Directories names
                                    List<string> dirsname = Directory.GetDirectories(Server.MapPath(Config.СheckValuePathToPhotos())).ToList<string>();

                                    FileStream CheckFileStream;
                                    Bitmap CheckBmp;

                                    List<string> filesname;

                                    // foreach inside foreach in order to check a new photo for its copies in all folders of all users
                                    foreach (string dir in dirsname)
                                    {
                                        filesname = Directory.GetFiles(dir).ToList<string>();
                                        foreach (string fl in filesname)
                                        {
                                            CheckFileStream = new FileStream(fl, FileMode.Open);
                                            CheckBmp = new Bitmap(CheckFileStream);
                                            CheckBmp = new Bitmap(CheckBmp, 64, 64);

                                            CheckFileStream.Close();

                                            if (_imagesService.CompareBitmapsFast(TempBmp, CheckBmp))
                                            {
                                                IsLoad = false;
                                                ViewBag.Error = "Photo already exists!";
                                                CheckBmp.Dispose();
                                                break;
                                            }
                                            else
                                                CheckBmp.Dispose();
                                        }
                                    }
                                }
                                else
                                {
                                    ViewBag.Error = "Photo created more than a year ago!";
                                    IsLoad = false;
                                }
                            }
                            else
                            {
                                ViewBag.Error = "Photo creation date not found!";
                                IsLoad = false;
                            }

                            if (IsLoad)
                            {
                                // extract only the filename
                                var OriginalFileName = Path.GetFileName(files.FileName);
                                // store the file inside User's folder
                                var OriginalPath = Path.Combine(DirPath, OriginalFileName);
                                //System.Windows.MessageBox.Show(OriginalPath);
                                files.SaveAs(OriginalPath);
                                System.IO.File.Delete(TempPath);
                            }
                            else
                            {
                                System.IO.File.Delete(TempPath);
                                return View("Error");
                            }

                        }
                        else
                        {
                            ViewBag.Error = "File too small!";
                            return View("Error");
                        }
                        // redirect back to the index action to show the form once again

                    }
                    else
                    {
                        ViewBag.Error = "Inappropriate format!";
                        return View("Error");
                    }

                }
                else
                {

                    return View();
                }
            }
            catch (Exception err)
            {

                ViewBag.Error = "Unexpected error: " + err.Message;
                return View("Error");

            }
            return RedirectToAction("Index");
        }
    }
}