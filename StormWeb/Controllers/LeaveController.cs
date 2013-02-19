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
    public class LeaveController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        #region Index

        public ViewResult Index()
        {
            return View(db.LeaveApplications.ToList());
        }
        public ActionResult ChangeStatus(int id, string comment, string status)
        {
            LeaveApplication leaveapplication = db.LeaveApplications.Single(x => x.Leave_Id == id);
            leaveapplication.Status = status;
            if (status == "Rejected")
            {
                leaveapplication.Comment = comment;
            }
            db.ObjectStateManager.ChangeObjectState(leaveapplication, EntityState.Modified);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        #endregion

        #region Details

        public ViewResult Details(int id)
        {
            LeaveApplication leaveapplication = db.LeaveApplications.Single(l => l.Leave_Id == id);
            return View(leaveapplication);
        }

        #endregion

        #region Create

        public ActionResult Create()
        {
            ViewBag.StaffName = new SelectList(db.Staffs, "Staff_Id", "FirstName");
            return View();
        }

        [HttpPost]
        public ActionResult Create(LeaveApplication leaveapplication, FormCollection fc)
        {
            if (ModelState.IsValid)
            {
                ViewBag.StaffName = db.Staffs.ToList();
                leaveapplication.Staff_Id = Convert.ToInt32(fc["FirstName"]);
                db.LeaveApplications.AddObject(leaveapplication);
                db.SaveChanges();
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added a new Leave Application " + leaveapplication.Reason + " for " + leaveapplication.Staff.FirstName + " " + leaveapplication.Staff.LastName), LogHelper.LOG_CREATE, LogHelper.SECTION_RESUME);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Created a new Leave Application" + leaveapplication.Reason);
                return RedirectToAction("Index");
            }

            return View(leaveapplication);
        }
        #endregion

        #region Edit

        public ActionResult Edit(int id)
        {
            LeaveApplication leaveapplication = db.LeaveApplications.Single(l => l.Leave_Id == id);
            if (leaveapplication != null)
                ViewBag.StaffName = new SelectList(db.Staffs, "Staff_Id", "FirstName", leaveapplication.Staff_Id);
            return View(leaveapplication);
        }

        [HttpPost]
        public ActionResult Edit(LeaveApplication leaveapplication, FormCollection fc)
        {
            if (ModelState.IsValid)
            {
                leaveapplication.Staff_Id = Convert.ToInt32(fc["FirstName"]);
                db.LeaveApplications.Attach(leaveapplication);
                db.ObjectStateManager.ChangeObjectState(leaveapplication, EntityState.Modified);
                db.SaveChanges();
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited the leave application of" + leaveapplication.Staff.FirstName + " " + leaveapplication.Staff.LastName), LogHelper.LOG_UPDATE, LogHelper.SECTION_RESUME);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Edited the leave application of " + leaveapplication.Staff.FirstName + " " + leaveapplication.Staff.LastName);

                return RedirectToAction("Index");
            }
            return View(leaveapplication);
        }

        #endregion

        #region Delete

        public ActionResult Delete(int id)
        {
            LeaveApplication leaveapplication = db.LeaveApplications.Single(l => l.Leave_Id == id);
            return View(leaveapplication);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            LeaveApplication leaveapplication = db.LeaveApplications.Single(l => l.Leave_Id == id);
            db.LeaveApplications.DeleteObject(leaveapplication);
            db.SaveChanges();
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully deleted the Leave Application of  " + leaveapplication.Staff.FirstName + " " + leaveapplication.Staff.LastName);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Deleted the Leave Application of " + leaveapplication.Staff.FirstName + " " + leaveapplication.Staff.LastName), LogHelper.LOG_DELETE, LogHelper.SECTION_LEAVE);
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