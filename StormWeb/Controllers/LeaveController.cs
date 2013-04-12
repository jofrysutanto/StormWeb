using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;

namespace StormWeb.Controllers
{ 
    public class LeaveController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /Leave/

        public ViewResult Index()
        {
            return View(db.LeaveApplications.ToList());
        }

        //
        // GET: /Leave/Details/5

        public ViewResult Details(int id)
        {
            LeaveApplication leaveapplication = db.LeaveApplications.Single(l => l.Leave_Id == id);
            return View(leaveapplication);
        }

        //
        // GET: /Leave/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Leave/Create

        [HttpPost]
        public ActionResult Create(LeaveApplication leaveapplication)
        {
            if (ModelState.IsValid)
            {
                db.LeaveApplications.AddObject(leaveapplication);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(leaveapplication);
        }
        
        //
        // GET: /Leave/Edit/5
 
        public ActionResult Edit(int id)
        {
            LeaveApplication leaveapplication = db.LeaveApplications.Single(l => l.Leave_Id == id);
            return View(leaveapplication);
        }

        //
        // POST: /Leave/Edit/5

        [HttpPost]
        public ActionResult Edit(LeaveApplication leaveapplication)
        {
            if (ModelState.IsValid)
            {
                db.LeaveApplications.Attach(leaveapplication);
                db.ObjectStateManager.ChangeObjectState(leaveapplication, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(leaveapplication);
        }

        //
        // GET: /Leave/Delete/5
 
        public ActionResult Delete(int id)
        {
            LeaveApplication leaveapplication = db.LeaveApplications.Single(l => l.Leave_Id == id);
            return View(leaveapplication);
        }

        //
        // POST: /Leave/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            LeaveApplication leaveapplication = db.LeaveApplications.Single(l => l.Leave_Id == id);
            db.LeaveApplications.DeleteObject(leaveapplication);
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