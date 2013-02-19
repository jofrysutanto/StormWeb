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

        [Authorize(Roles = "Counsellor,Super,BranchManager,Administrator,Finance")]
        public ViewResult Index()
        {
            var universities = db.Universities.Include("Country");
            return View(universities.ToList());
        }


        [Authorize(Roles = "Super,BranchManager,Administrator")]
        public ViewResult Details(int id)
        {
            University university = db.Universities.Single(u => u.University_Id == id);
            
            return View(university);
        }

        #region CREATE

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        public ActionResult Create()
        {
            ViewBag.CountryName = new SelectList(db.Countries, "Country_Id", "Country_Name");

            return View();
        }

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(University university, FormCollection fc)
        {
           if (ModelState.IsValid)
            {
                db.Universities.AddObject(university);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CountryName = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username+" Added a New University" + university.University_Name), LogHelper.LOG_CREATE, LogHelper.SECTION_UNIVERSITY);
            return View(university);
        }

        #endregion

        #region EDIT

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        public ActionResult Edit(int id)
        {
            University university = db.Universities.Single(u => u.University_Id == id);
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            return View(university);
        }
        
        [Authorize(Roles = "Super,BranchManager,Administrator")]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(University university)
        {
            //var uni = db.Universities.SingleOrDefault(x=>x.University_Id==university.University_Id);
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            if (ModelState.IsValid)
            {
                university.Country_Id = university.Country.Country_Id;
                db.Universities.Attach(university);
                db.ObjectStateManager.ChangeObjectState(university, EntityState.Modified);
                db.SaveChanges();
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited the details of the University" + university.University_Name), LogHelper.LOG_UPDATE, LogHelper.SECTION_UNIVERSITY);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Edited the University details" + university.University_Name);
                return RedirectToAction("Index");
            } 
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited the details of the University" + university.University_Name), LogHelper.LOG_UPDATE, LogHelper.SECTION_UNIVERSITY);
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Edited the University details" + university.University_Name);
            return View(university);
        }

        #endregion

        #region DELETE

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        public ActionResult Delete(int id)
        {
            University university = db.Universities.Single(u => u.University_Id == id);
            return View(university);
        }

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            University university = db.Universities.Single(u => u.University_Id == id);
            try
            {
                db.Universities.DeleteObject(university);
                db.SaveChanges();
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Deleted the University " + university.University_Name), LogHelper.LOG_DELETE, LogHelper.SECTION_UNIVERSITY);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Created a new branch" + university.University_Name);
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                ModelState.AddModelError(String.Empty, "Cannot delete");
                return View(university);
            }

        }

        #endregion

        public ActionResult Faculties(int id)
        {
            var fac = db.Faculties.Where(x => x.University_Id == id);
            ViewBag.UniversityId = id;
            return View(fac.ToList());
        }

        #region CREATE FACULTY

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        public ActionResult CreateFaculty(int id)
        {
            var university = db.Universities.SingleOrDefault(u => u.University_Id == id);
            ViewBag.UniId = university;
            //ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", university.Country_Id);
            return View();
        }

        [HttpPost]
        public ActionResult CreateFaculty(int id, Faculty fac, FormCollection fc)
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
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username+" Added a New Faculty" + fac.Faculty_Name), LogHelper.LOG_CREATE, LogHelper.SECTION_FACULTY);
            return View();
        }

        #endregion

        #region EDIT FACULTY

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
                fac.University.University_Id = universityid;
                db.Faculties.Attach(fac);
                db.ObjectStateManager.ChangeObjectState(fac, EntityState.Modified);
                db.SaveChanges();
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "You have successfully edited Faculty details");
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username+" Edited the details of the Faculty" + fac.Faculty_Name), LogHelper.LOG_UPDATE, LogHelper.SECTION_FACULTY);
                return RedirectToAction("Faculties", new { id = universityid });
            }
            return View();
        }

        #endregion

        #region DESCRIPTION

        public ActionResult Description(int id)
        {
            University university = db.Universities.SingleOrDefault(u => u.University_Id == id);

            if (university == null)
                return View(new University());

            return View(university);
        }

        #endregion

        #region DELETE FACULTY

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
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username+" Deleted the Faculty" + f.Faculty_Name ), LogHelper.LOG_DELETE, LogHelper.SECTION_FACULTY);
            return RedirectToAction("Faculties", new { id = universityId });
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}