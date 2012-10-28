using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Helper;
using System.Collections;
using System.Diagnostics;

namespace StormWeb.Controllers
{ 
    public class BranchController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /Branch/
        [Authorize(Roles="Super")]
        public ViewResult Index()
        {

            var branches = db.Branches.Include("Address");            
            return View(branches.ToList());
           
        }

        //
        // GET: /Branch/Details/5
        [Authorize(Roles = "Super")]
        public ViewResult Details(int id)
        {
            Branch branch = db.Branches.Single(b => b.Branch_Id == id);
            return View(branch);
        }

        //
        // GET: /Branch/Create
         [Authorize(Roles = "Super")]
        public ActionResult Create()
        {
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name");
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name");
            return View();
        } 

        //
        // POST: /Branch/Create
         [Authorize(Roles = "Super")]
        [HttpPost]
        public ActionResult Create(Branch branch)
        {
            if (ModelState.IsValid)
            {
                db.Branches.AddObject(branch);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name", branch.Address_Id);
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", branch.Address.Country_Id);
            return View(branch);
        }
        
        //
        // GET: /Branch/Edit/5
 
        //public ActionResult Edit(int id)
         [Authorize(Roles = "Super")]
        public ActionResult Edit(int id)
        {
            BranchModel branchmodel = new BranchModel();
            int branchId = id;
            var branches = db.Branches.ToList().Where(x => x.Branch_Id == branchId);
            branchmodel.branchTable = branches.ToList();
            
            return View(branchmodel);
        }

        //
        // POST: /Branch/Edit/5

        [HttpPost]
        [Authorize(Roles = "Super")]
        public ActionResult Edit(BranchModel branchmodel,FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                Branch branches = new Branch();
                int branchid = Convert.ToInt32(collection["item.Branch_Id"]);
                branches = db.Branches.Single(x => x.Branch_Id == branchid);

                Address address = branches.Address;
                Country country = branches.Address.Country;



                //db.Branches.Attach(branch);
                db.ObjectStateManager.ChangeObjectState(branches, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name", branch.Address_Id);
            //ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", branch.Address.Country_Id);
            return View(branchmodel);
        }

        //
        // GET: /Branch/Delete/5
  [Authorize(Roles = "Super")]
        public ActionResult Delete(int id)
        {
            Branch branch = db.Branches.Single(b => b.Branch_Id == id);
            return View(branch);
        }

        //
        // POST: /Branch/Delete/5

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Super")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Branch branch = db.Branches.Single(b => b.Branch_Id == id);
            db.Branches.DeleteObject(branch);
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