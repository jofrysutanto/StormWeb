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
        public ActionResult Create(Branch branch, FormCollection fc)
        {
            branch.Address.Country_Id = Convert.ToInt32(fc["Country_Id"]);

            if (ModelState.IsValid)
            {
                db.Branches.AddObject(branch);
                db.SaveChanges();
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added a new branch " + branch.Branch_Name), LogHelper.LOG_CREATE, LogHelper.SECTION_BRANCH);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Created a new branch" + branch.Branch_Name);
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

            List<Country> c = new List<Country>();
            c.Add(branchmodel.branchTable.First().Address.Country);
            branchmodel.countryTable = c;
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
                int addressId = Convert.ToInt32(collection["Address_Id"]);
                int countryId = Convert.ToInt32(collection["Country_Id"]);
                branches.Branch_Id = branchid;

                branchmodel.branchTable = db.Branches.ToList().Where(x => x.Branch_Id == branchid).ToList();
                branchmodel.addressTable = db.Addresses.ToList().Where(x => x.Address_Id == addressId).ToList();
                branchmodel.countryTable = db.Countries.ToList().Where(x => x.Country_Id == countryId).ToList();

                branches = db.Branches.Single(x => x.Branch_Id == branchid);

                Address address =branches.Address;
                branches.Address= db.Addresses.Single(x => x.Address_Id == addressId);
                address = branches.Address;

                Country country = branches.Address.Country;
                branches.Address.Country = db.Countries.Single(x => x.Country_Id == countryId);
                country = branches.Address.Country;

                if (collection["Address_Id"] == "" || collection["Address_Id"] == null)
                {
                    ModelState.AddModelError("Address", "Please Enter Your Address");
                    return View(branchmodel);
                }
                if (collection["Branch_Name"] == "" || collection["Branch_Name"] == null)
                {
                    ModelState.AddModelError("Branch_Name", "Please Enter Your Branch Name");
                    return View(branchmodel);
                }
                if (collection["Address_Name"] == "" || collection["Address_Name"] == null)
                {
                    ModelState.AddModelError("Address_Name", "Please Enter Your Address Name");
                    return View(branchmodel);
                }
                if (collection["City"] == "" || collection["City"] == null)
                {
                    ModelState.AddModelError("City", "Please Enter Your City");
                    return View(branchmodel);
                }
                if (collection["State"] == "" || collection["State"] == null)
                {
                    ModelState.AddModelError("State", "Please Enter Your State");
                    return View(branchmodel);
                }
                //if (collection["Zipcode"] == "" || collection["Zipcode"] == null)
                //{
                //    ModelState.AddModelError("Zipcode", "Please Enter Your Zipcode");
                //    return View(branchmodel);
                //}
                if (collection["Primary_Contact"] == "" || collection["Primary_Contact"] == null)
                {
                    ModelState.AddModelError("Primary_Contact", "Please Enter Your Primary Contact");
                    return View(branchmodel);
                }
                if (collection["Seconday_Contact"] == "" || collection["Secondary_Contact"] == null)
                {
                    ModelState.AddModelError("Seconday_Contact", "Please Enter Your Seconday Contact");
                    return View(branchmodel);
                }
                if (collection["Email"] == "" || collection["Email"] == null)
                {
                    ModelState.AddModelError("Email", "Please Enter Your Email");
                    return View(branchmodel);
                } 

                string str = collection["Address_Name"];

                branches.Address.Address_Name = collection["Address_Name"];
                branches.Address.City = collection["City"];
                branches.Address.State = collection["State"];
                branches.Address.Zipcode = Convert.ToInt32(collection["Zipcode"]);
                branches.Branch_Name = collection["Branch_Name"];
                branches.Primary_Contact = Convert.ToInt32(collection["Primary_Contact"]);
                branches.Secondary_Contact = Convert.ToInt32(collection["Seconday_Contact"]);
                branches.Email = collection["Email"];
                 
                db.ObjectStateManager.ChangeObjectState(address, EntityState.Modified);
                db.ObjectStateManager.ChangeObjectState(country, EntityState.Modified);
                db.ObjectStateManager.ChangeObjectState(branches, EntityState.Modified);

                db.SaveChanges();
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited the branch " + branches.Branch_Name), LogHelper.LOG_UPDATE, LogHelper.SECTION_BRANCH);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Edited branch" + branches.Branch_Name);
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
            Branch branch = db.Branches.SingleOrDefault(b => b.Branch_Id == id);
            Appointment appointment = db.Appointments.SingleOrDefault(x => x.Branch_Id == id);
            Case cases = db.Cases.SingleOrDefault(x => x.Branch_Id == id);
            Branch_Staff branchStaff = db.Branch_Staff.SingleOrDefault(x => x.Branch_Id == id);
            Client client = db.Clients.SingleOrDefault(x => x.Branch_Id == id);
            General_Enquiry generalEnquiry = db.General_Enquiry.SingleOrDefault(x => x.Branch_Id == id);

            if (appointment != null)
            {
                ModelState.AddModelError("Branch", "Branch " + branch.Branch_Name + " cannot be deleted Since there are Appointment's assigned to that branch");
                NotificationHandler.setNotification(NotificationHandler.NOTY_WARNING, "Branch " + branch.Branch_Name + " cannot be deleted Since there are Appointment's assigned to that branch");
                return View(branch);
            }
            if (cases != null)
            {
                ModelState.AddModelError("Branch", "Branch " + branch.Branch_Name + " cannot be deleted Since there are Case's assigned to that branch");
                NotificationHandler.setNotification(NotificationHandler.NOTY_WARNING, "Branch " + branch.Branch_Name + " cannot be deleted Since there are Case's assigned to that branch");
                return View(branch);
            }
            if (branchStaff != null)
            {
                ModelState.AddModelError("Branch", "Branch " + branch.Branch_Name + " cannot be deleted Since there are Staff's assigned to that branch");
                NotificationHandler.setNotification(NotificationHandler.NOTY_WARNING, "Branch " + branch.Branch_Name + " cannot be deleted Since there are Staff's assigned to that branch");
                return View(branch);
            }
            if (client != null)
            {
                ModelState.AddModelError("Branch", "Branch " + branch.Branch_Name + " cannot be deleted Since there are Client's assigned to that branch");
                NotificationHandler.setNotification(NotificationHandler.NOTY_WARNING, "Branch " + branch.Branch_Name + " cannot be deleted Since there are Client's assigned to that branch");
                return View(branch);
            }
            if (generalEnquiry != null)
            {
                ModelState.AddModelError("Branch", "Branch " + branch.Branch_Name + " cannot be deleted Since there are Enquiries related to that branch");
                NotificationHandler.setNotification(NotificationHandler.NOTY_WARNING, "Branch " + branch.Branch_Name + " cannot be deleted Since there are Enquiries related to that branch");
                return View(branch);
            }

            db.Branches.DeleteObject(branch);
            db.SaveChanges();
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Branch " + branch.Branch_Name + " is Deleted");
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