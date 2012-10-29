using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Helper;
using System.Diagnostics;
//Doing modification no success
namespace StormWeb.Controllers
{
    public class StudentController : Controller
    {
        private StormDBEntities db = new StormDBEntities();
        int staffId = 0;

        [Authorize(Roles = "Super,Counsellor")]
        public ViewResult Index()
        {


            if (CookieHelper.isStaff())
            {
                staffId = Convert.ToInt32(CookieHelper.StaffId);
            }

            var cases = db.Case_Staff.ToList().Where(c => c.Staff_Id == staffId);

            ViewBag.cases = cases;

            var students = db.Students.Include("Client");
            //return View(students.ToList());
            //return View(caseapp);
            return View();

        }

        [Authorize(Roles = "Super,Counsellor")]
        public ViewResult Details(int id)
        {
            Student student = db.Students.Single(s => s.Student_Id == id);
            return View(student);
        }

        #region CREATE

        [Authorize(Roles = "Super,Counsellor")]
        public ActionResult Create()
        {
            ViewBag.Client_Id = new SelectList(db.Clients, "Client_Id", "Client_Id");

            return View();
        }

        [HttpPost]
        public ActionResult Create(Student student)
        {
            if (ModelState.IsValid)
            {
                db.Students.AddObject(student);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Client_Id = new SelectList(db.Clients, "Client_Id", "Client_Id", student.Client_Id);
            LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added a New Student" + student.Client.GivenName + " " + student.Client.LastName ), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
            return View(student);
        }

        #endregion

        #region EDIT

        [Authorize(Roles = "Super,Counsellor")]
        public ActionResult Edit(int id)
        {
            Student student = db.Students.Single(s => s.Student_Id == id);
            ViewBag.Client_Id = new SelectList(db.Clients, "Client_Id", "Client_Id", student.Client_Id);
            return View(student);
        }

        [HttpPost]
        public ActionResult Edit(Student student)
        {
            if (ModelState.IsValid)
            {
                db.Students.Attach(student);
                db.ObjectStateManager.ChangeObjectState(student, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Client_Id = new SelectList(db.Clients, "Client_Id", "Client_Id", student.Client_Id);
            LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username+" Edited the Details of the Student "+ student.Client.GivenName + " " + student.Client.LastName ), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
            return View(student);
        }

        #endregion

        #region DELETE

        [Authorize(Roles = "Super,Counsellor")]
        public ActionResult Delete(int id)
        {
            Student student = db.Students.Single(s => s.Student_Id == id);
            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Student student = db.Students.Single(s => s.Student_Id == id);
            db.Students.DeleteObject(student);
            db.SaveChanges();
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username +" Deleted the Student " + student.Client.GivenName + " " + student.Client.LastName ), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
            return RedirectToAction("Index");
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}