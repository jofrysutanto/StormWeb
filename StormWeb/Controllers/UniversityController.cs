// --------------------------------------------------------------------------------------------------------------------
// <summary>
// File Name    : UniversityController.cs
// Created Date : 14/08/2011
// Created By   : Manali Modi
// Description  : All information related to the universities.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using System.Collections;
using System.Diagnostics;
using StormWeb.Helper;

namespace StormWeb.Controllers
{ 
    public class UniversityController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /University/
         [Authorize(Roles = "Super")]
        public ViewResult Index()
        {
            var universities = db.Universities.Include("Country");
            return View(universities.ToList());
        }
         

        //
        // GET: /University/Details/5
         [Authorize(Roles = "Super")]
        public ViewResult Details(int id)
        {
            University university = db.Universities.Single(u => u.University_Id == id);
            return View(university);
        }

        //
        // GET: /University/Create
         [Authorize(Roles = "Super")]
        public ActionResult Create()
        {
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name");
           
            return View();
        } 

        //
        // POST: /University/Create
         [Authorize(Roles = "Super")]
        [HttpPost]
        public ActionResult Create(University university,FormCollection fc)
        {          
            if (ModelState.IsValid)
            {
                db.Universities.AddObject(university);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            return View(university);
        }
        
        //
        // GET: /University/Edit/5
        [Authorize(Roles = "Super")]
        public ActionResult Edit(int id)
        {
            University university = db.Universities.Single(u => u.University_Id == id);
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            return View(university);
        }

        //
        // POST: /University/Edit/5
         [Authorize(Roles = "Super")]
        [HttpPost]
        public ActionResult Edit(University university)
        {
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);            
            if (ModelState.IsValid)
            {
                db.Universities.Attach(university);
                db.ObjectStateManager.ChangeObjectState(university, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
         
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            return View(university);
        }

        //
        // GET: /University/Delete/5
         [Authorize(Roles = "Super")]
        public ActionResult Delete(int id)
        {
            University university = db.Universities.Single(u => u.University_Id == id);
            return View(university);
        }

        //
        // POST: /University/Delete/5
         [Authorize(Roles = "Super")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            University university = db.Universities.Single(u => u.University_Id == id);
            try
            {
                db.Universities.DeleteObject(university);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                ModelState.AddModelError(String.Empty, "Cannot delete");
                return View(university);
            }
            
        }

        public ActionResult Faculties(int id)
        {
            var fac = db.Faculties.Where(x => x.University_Id == id);
            ViewBag.UniversityId = id;
            return View(fac.ToList());
        }

        [Authorize(Roles = "Super")]
        public ActionResult CreateFaculty(int id)
        {
            var university = db.Universities.SingleOrDefault(u => u.University_Id == id);
            ViewBag.UniId = university;
            //ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            return View();
        }

        //
        // POST: /University/Edit/5
        [HttpPost]
        public ActionResult CreateFaculty(int id,Faculty fac,FormCollection fc)
        {
            //ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            
            //ViewBag.UniId = university.ToList();
            var university = db.Universities.SingleOrDefault(u => u.University_Id == id);
            ViewBag.UniId = university;
            if (ModelState.IsValid)
            {
                db.Faculties.AddObject(fac);
                db.SaveChanges();
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "You have successfully created a Faculty");
                return RedirectToAction("Index");
            }


            //ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            return View();
        }

        public ActionResult EditFaculty(int id)
        {
            var Faculty = db.Faculties.Single(f => f.Faculty_Id == id);
            ViewBag.FacId = Faculty;
            
            return View(Faculty);
        }

        [HttpPost]
        public ActionResult EditFaculty(int id, Faculty fac, FormCollection fc)
        {
            int universityid = fac.University_Id;
            if (ModelState.IsValid)
            {
                db.Faculties.Attach(fac);
                db.ObjectStateManager.ChangeObjectState(fac, EntityState.Modified);
                db.SaveChanges();
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "You have successfully edited Faculty details");
                return RedirectToAction("Faculties", new { id = universityid });
            }
            return View();
        }

        public ActionResult DeleteFaculty(int id)
        {
            Faculty fac = db.Faculties.Single(f => f.Faculty_Id == id);
            var courses = db.Courses.Where(c => c.Faculty_Id == fac.Faculty_Id);
            int countCourses = courses.Count();
            ViewBag.CoursesCount = countCourses;
            return View(fac);
        }

        public ActionResult ConfirmDeleteFaculty(int id)
        {
            Faculty f = db.Faculties.DefaultIfEmpty(null).SingleOrDefault(x => x.Faculty_Id == id);

            if (f == null)
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Faculty does not exist");
                return RedirectToAction("Index");
            }

            int universityId = f.University_Id;

            try
            {
                db.Faculties.DeleteObject(f);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Error deleting Faculty");
                return RedirectToAction("Index");
            }

            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Faculty deleted");
            return RedirectToAction("Faculties", new { id = universityId });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}