﻿using System;
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

        [Authorize(Roles = "Super,BranchManager")]
        public ViewResult Index()
        {

            var branches = db.Branches.Include("Address");
            return View(branches.ToList());

        }

        [Authorize(Roles = "Super,BranchManager")]
        public ViewResult Details(int id)
        {
            Branch branch = db.Branches.Single(b => b.Branch_Id == id);
            return View(branch);
        }

        #region CREATE

        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult Create()
        {
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name");
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name");
            return View();
        }

        [Authorize(Roles = "Super,BranchManager")]
        [HttpPost]
        public ActionResult Create(Branch branch)
        {
            if (ModelState.IsValid)
            {
                db.Branches.AddObject(branch);
                db.SaveChanges();
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added a new branch " + branch.Branch_Name), LogHelper.LOG_CREATE, LogHelper.SECTION_BRANCH);
                return RedirectToAction("Index");
            }

            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name", branch.Address_Id);
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", branch.Address.Country_Id);
            return View(branch);
        }

        #endregion

        #region EDIT

        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult Edit(int id)
        {
            BranchModel branchmodel = new BranchModel();
            int branchId = id;
            var branches = db.Branches.ToList().Where(x => x.Branch_Id == branchId);
            branchmodel.branchTable = branches.ToList();

            List<Address> l = new List<Address>();

            l.Add(branchmodel.branchTable.First().Address);

            branchmodel.addressTable = l;

            return View(branchmodel);
        }

        [HttpPost]
        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult Edit(BranchModel branchmodel, FormCollection collection)
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
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited the branch " + branches.Branch_Name), LogHelper.LOG_UPDATE, LogHelper.SECTION_BRANCH);

                return RedirectToAction("Index");
            }

            //ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name", branch.Address_Id);
            //ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name", branch.Address.Country_Id);
            return View(branchmodel);
        }

        #endregion

        #region DELETE

        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult Delete(int id)
        {
            Branch branch = db.Branches.Single(b => b.Branch_Id == id);
            return View(branch);
        }


        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult DeleteConfirmed(int id)
        {
            Branch branch = db.Branches.Single(b => b.Branch_Id == id);
            db.Branches.DeleteObject(branch);
            db.SaveChanges();
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Deleted the branch " + branch.Branch_Name), LogHelper.LOG_DELETE, LogHelper.SECTION_BRANCH);
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