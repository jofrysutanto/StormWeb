/* Author: Maysa Labadi
 * Date: 29/10/2012
 * 
 * Payment Controller
 * 
 * Handles all unpaid applications for students * 
 * 
 * */

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
    public class PaymentController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /Payment/

        public ViewResult Index()
        {
            
            PaymentViewModel model = new PaymentViewModel();

            var unpaidApplications = db.Applications.Where(p => p.Status == "Offer_Letter").ToList();
            var paidApplications = db.Applications.Where(p => p.Status == "Payment_Received").ToList();

            model.unpaidApplications = unpaidApplications;
            model.paidApplications = paidApplications;
            return View(model);
        }

        //
        // GET: /Payment/Details/5

        public ViewResult Details(int id)
        {
            Payment payment = db.Payments.Single(p => p.Id == id);
            return View(payment);
        }

        //
        // GET: /Payment/Create

        public ActionResult Create(int id)
        {
            ViewBag.Application_Id = id;
            ViewBag.Approved_By = StormWeb.Helper.CookieHelper.getStaffId();
            return View();
        } 

        //
        // POST: /Payment/Create

        [HttpPost]
        public ActionResult Create(int id, Payment payment)
        {
            
            if (ModelState.IsValid)
            {
                payment.Application_Id = id;
                db.Payments.AddObject(payment);
                Application app = db.Applications.Single(a => a.Application_Id == id);
                app.Status = "Payment_Received";
                db.SaveChanges();
                return View("Refresh");
            }

            return View(payment);
        }
        
        //
        // GET: /Payment/Edit/5
 
        public ActionResult Edit(int id)
        {
            Payment payment = db.Payments.Single(p => p.Id == id);
            ViewBag.Application_Id = new SelectList(db.Applications, "Application_Id", "Status", payment.Application_Id);
            ViewBag.Approved_By = new SelectList(db.Staffs, "Staff_Id", "Title", payment.Approved_By);
            return View(payment);
        }

        //
        // POST: /Payment/Edit/5

        [HttpPost]
        public ActionResult Edit(Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Payments.Attach(payment);
                db.ObjectStateManager.ChangeObjectState(payment, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Application_Id = new SelectList(db.Applications, "Application_Id", "Status", payment.Application_Id);
            ViewBag.Approved_By = new SelectList(db.Staffs, "Staff_Id", "Title", payment.Approved_By);
            return View(payment);
        }

        //
        // GET: /Payment/Delete/5
 
        public ActionResult Delete(int id)
        {
            Payment payment = db.Payments.Single(p => p.Id == id);
            return View(payment);
        }

        //
        // POST: /Payment/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Payment payment = db.Payments.Single(p => p.Id == id);
            db.Payments.DeleteObject(payment);
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