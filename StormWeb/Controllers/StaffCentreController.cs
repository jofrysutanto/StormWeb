using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Helper;

namespace StormWeb.Controllers
{
    public class StaffCentreController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        [Authorize(Roles = "Counsellor,Admission,Visa,Super,BranchManager")]
        public ViewResult Index()
        {
            int staffId = Convert.ToInt32(CookieHelper.StaffId);

            ViewBag.CountAppointment = Utilities.getNumberOfAppointment(Convert.ToInt32(CookieHelper.StaffId));
            ViewBag.CountNewEnquiries = Utilities.getNumberOfNewEnquiries(Convert.ToInt32(CookieHelper.StaffId));
            ViewBag.CountNewStudents = Utilities.getNumberOfNewStudents(Convert.ToInt32(CookieHelper.StaffId));

            ViewBag.Students = StudentsHelper.getStudentsListFromCookie();

            ViewBag.StudentStaff = StudentStaffController.getStudentStaffModel();

            Staff s = db.Staffs.Single(x => x.Staff_Id == staffId);

            return View(s);
        }

        [Authorize(Roles = "Counsellor,Admission,Visa,Super,BranchManager")]
        public ViewResult Details(int id)
        {
            Staff staff = db.Staffs.Single(s => s.Staff_Id == id);
            return View(staff);
        }

        [Authorize(Roles = "Counsellor,Admission,Visa,Super,BranchManager")]
        public ActionResult Profile()
        {
            if (CookieHelper.isStaff())
            {
                int staffID = Convert.ToInt32(CookieHelper.StaffId);

                return RedirectToAction("Details", "Staff", new { id = staffID });
            }

            TempData[AccountController.BAD_LINK] = true;
            return RedirectToAction("Logon", "Account");
        }

        #region CREATE

        [Authorize(Roles = "Counsellor,Admission,Visa,Super,BranchManager")]
        public ActionResult Create()
        {
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name");
            ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name");
            return View();
        }

        [Authorize(Roles = "Counsellor,Admission,Visa,Super,BranchManager")]
        [HttpPost]
        public ActionResult Create(Staff staff)
        {
            if (ModelState.IsValid)
            {
                db.Staffs.AddObject(staff);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name", staff.Address_Id);
            ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
            return View(staff);
        }

        #endregion

        #region EDIT

        [Authorize(Roles = "Counsellor,Admission,Visa,Super,BranchManager")]
        public ActionResult Edit(int id)
        {
            Staff staff = db.Staffs.Single(s => s.Staff_Id == id);
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name", staff.Address_Id);
            ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
            return View(staff);
        }

        [Authorize(Roles = "Counsellor,Admission,Visa,Super,BranchManager")]
        [HttpPost]
        public ActionResult Edit(Staff staff)
        {
            if (ModelState.IsValid)
            {
                db.Staffs.Attach(staff);
                db.ObjectStateManager.ChangeObjectState(staff, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name", staff.Address_Id);
            ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
            return View(staff);
        }

        #endregion

        #region DELETE

        [Authorize(Roles = "Counsellor,Admission,Visa,Super,BranchManager")]
        public ActionResult Delete(int id)
        {
            Staff staff = db.Staffs.Single(s => s.Staff_Id == id);
            return View(staff);
        }

        [Authorize(Roles = "Counsellor,Admission,Visa,Super,BranchManager")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Staff staff = db.Staffs.Single(s => s.Staff_Id == id);
            db.Staffs.DeleteObject(staff);
            db.SaveChanges();
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