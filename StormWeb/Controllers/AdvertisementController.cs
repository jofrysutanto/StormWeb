﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Helper;
using System.IO;

namespace StormWeb.Controllers
{ 
    public class AdvertisementController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /Advertisement/

        public ViewResult Index()
        {
            return View(db.Advertisements.ToList());
        }

        //
        // GET: /Advertisement/Details/5

        public ViewResult Details(int id)
        {
            Advertisement advertisement = db.Advertisements.Single(a => a.AdvertisementId == id);
            return View(advertisement);
        }

        //
        // GET: /Advertisement/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Advertisement/Create
        public static string MARKETING_UPLOADS_PATH = "Marketing/";

        [HttpPost]
        public ActionResult Create(Advertisement advertisement,HttpPostedFile file)
        {
            string name = StormWeb.Helper.Utilities.getName(CookieHelper.Username);
            string fileToCreate = MARKETING_UPLOADS_PATH + CookieHelper.StudentId + "_" + name + '/' + Path.GetFileNameWithoutExtension(file.FileName) + "_Marketing_" + advertisement.AdvertisementId + Path.GetExtension(file.FileName);
            if (ModelState.IsValid)
            {
                db.Advertisements.AddObject(advertisement);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(advertisement);
        }
        
        //
        // GET: /Advertisement/Edit/5
 
        public ActionResult Edit(int id)
        {
            Advertisement advertisement = db.Advertisements.Single(a => a.AdvertisementId == id);
            return View(advertisement);
        }

        //
        // POST: /Advertisement/Edit/5

        [HttpPost]
        public ActionResult Edit(Advertisement advertisement)
        {
            if (ModelState.IsValid)
            {
                db.Advertisements.Attach(advertisement);
                db.ObjectStateManager.ChangeObjectState(advertisement, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(advertisement);
        }

        //
        // GET: /Advertisement/Delete/5
 
        public ActionResult Delete(int id)
        {
            Advertisement advertisement = db.Advertisements.Single(a => a.AdvertisementId == id);
            return View(advertisement);
        }

        //
        // POST: /Advertisement/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Advertisement advertisement = db.Advertisements.Single(a => a.AdvertisementId == id);
            db.Advertisements.DeleteObject(advertisement);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}