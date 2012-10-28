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
    public class EventController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /Event/

        public ViewResult Index()
        {
            return View(db.Events.ToList());
        }

        //
        // GET: /Event/Details/5

        public ViewResult Details(int id)
        {
            Event event1 = db.Events.Single(e => e.EventId == id);
            return View(event1); 

        }

        //
        // GET: /Event/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Event/Create

        [HttpPost]
        public ActionResult Create(Event event1)
        {
            if (ModelState.IsValid)
            {
                db.Events.AddObject(event1);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(event1);
        }
        
        //
        // GET: /Event/Edit/5
 
        public ActionResult Edit(int id)
        {
            Event event1 = db.Events.Single(e => e.EventId == id);
            return View(event1);
        }

        //
        // POST: /Event/Edit/5

        [HttpPost]
        public ActionResult Edit(Event event1)
        {
            if (ModelState.IsValid)
            {
                db.Events.Attach(event1);
                db.ObjectStateManager.ChangeObjectState(event1, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(event1);
        }

        //
        // GET: /Event/Delete/5
 
        public ActionResult Delete(int id)
        {
            Event event1 = db.Events.Single(e => e.EventId == id);
            return View(event1);
        }

        //
        // POST: /Event/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Event event1 = db.Events.Single(e => e.EventId == id);
            db.Events.DeleteObject(event1);
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