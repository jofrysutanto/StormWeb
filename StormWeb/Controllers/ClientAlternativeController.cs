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
    public class ClientAlternativeController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /ClientAlternative/

        public ViewResult Index()
        {
            var clients = db.Clients.Include("Address");
            return View(clients.ToList());
        }

        //
        // GET: /ClientAlternative/Details/5

        public ViewResult Details(int id)
        {
            Client client = db.Clients.Single(c => c.Client_Id == id);
            return View(client);
        }

        //
        // GET: /ClientAlternative/Create

        public ActionResult Create()
        {
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name");
            return View();
        } 

        //
        // POST: /ClientAlternative/Create

        [HttpPost]
        public ActionResult Create(Client client)
        {
            if (ModelState.IsValid)
            {
                db.Clients.AddObject(client);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name", client.Address_Id);
            return View(client);
        }
        
        //
        // GET: /ClientAlternative/Edit/5
 
        public ActionResult Edit(int id)
        {
            Client client = db.Clients.Single(c => c.Client_Id == id);
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name", client.Address_Id);
            return View(client);
        }

        //
        // POST: /ClientAlternative/Edit/5

        [HttpPost]
        public ActionResult Edit(Client client)
        {
            if (ModelState.IsValid)
            {
                db.Clients.Attach(client);
                db.ObjectStateManager.ChangeObjectState(client, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name", client.Address_Id);
            return View(client);
        }

        //
        // GET: /ClientAlternative/Delete/5
 
        public ActionResult Delete(int id)
        {
            Client client = db.Clients.Single(c => c.Client_Id == id);
            return View(client);
        }

        //
        // POST: /ClientAlternative/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Client client = db.Clients.Single(c => c.Client_Id == id);
            db.Clients.DeleteObject(client);
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