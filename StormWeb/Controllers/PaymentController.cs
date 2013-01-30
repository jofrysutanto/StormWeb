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
using StormWeb.Helper;
using System.Xml;

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



            var unpaidApplications = (from app in db.Applications
                                     from p in app.Payments
                                     where p.Approved_By == null
                                     select app).ToList();

            var paidApplications = (from app in db.Applications
                                   from p in app.Payments
                                   where p.Approved_By != null
                                   select app).ToList();

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

        
        public ActionResult Create(int id)
        {
            ViewBag.Application_Id = id;

            ViewBag.curr = new SelectList(StormWeb.Models.ModelHelper.Currency.getCurrencyFromXml(), "curr", "curr");

            ViewBag.PaymentMethod = new SelectList(PaymentHelper.GetPaymentType(), "MethodType", "MethodType");
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
                payment.Approved_By = StormWeb.Helper.CookieHelper.Username;
                db.Payments.AddObject(payment);
                
                Application app = db.Applications.Single(a => a.Application_Id == id);
                app.Status = "Payment_Received";

                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Payment added   " + payment.Id), LogHelper.LOG_CREATE, LogHelper.SECTION_PAYMENT);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Payment transaction was created!");

                return View("Refresh");
            }

            return View(payment);
        }
        
        //
        // GET: /Payment/Edit/5
 
        public ActionResult Edit(int id)
        {
            Payment payment = db.Payments.Single(p => p.Id == id);
            payment.Approved_By = StormWeb.Helper.CookieHelper.Username;
            ViewBag.PaymentMethod = new SelectList(PaymentHelper.GetPaymentType(), "MethodType", "MethodType");
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
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Payment Modified   " + payment.Id), LogHelper.LOG_UPDATE, LogHelper.SECTION_PAYMENT);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Payment transaction was modified!");

                return View("Refresh");
            }

            return View(payment);
        }

        //
        // GET: /Payment/Delete/5
 
        //public ActionResult Delete(int id)
        //{
        //    Payment payment = db.Payments.Single(p => p.Id == id);
        //    return View(payment);
        //}

        ////
        //// POST: /Payment/Delete/5

        //[HttpPost, ActionName("Delete")]
        public ActionResult Delete(int id, int appId)
        {            
            Payment payment = db.Payments.Single(p => p.Id == id);
            db.Payments.DeleteObject(payment);
            Application app = db.Applications.Single(a => a.Application_Id == appId);
            app.Status = "Offer_Letter";
            db.ObjectStateManager.ChangeObjectState(app, EntityState.Modified);
            db.SaveChanges();
            LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Payment Deleted   " + payment.Id), LogHelper.LOG_DELETE, LogHelper.SECTION_PAYMENT);

            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Payment transaction was deleted!");

            return RedirectToAction("Index");
        }
        

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}