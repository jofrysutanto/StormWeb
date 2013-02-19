using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using System.Web.Security;
using StormWeb.Helper;
using StormWeb.Models.ModelHelper;

namespace StormWeb.Controllers
{
    public class AssociateController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /Associate/

        public ViewResult Index()
        {
            var associates = db.Associates;
            return View(associates.ToList());
        }

        public ViewResult MAssociate()
        {
            int StaffId = Convert.ToInt32(CookieHelper.getStaffId());
            var associates = db.Associates.Where(x => x.Referrer == StaffId).ToList();
            return View(associates.ToList());
        }

        //
        // GET: /Associate/Details/5

        public ViewResult Details(int id)
        {
            Associate associate = db.Associates.Single(a => a.AssociateId == id);
            return View(associate);
        }

        //
        // GET: /Associate/Create

        public ActionResult Create()
        {
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name");
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name");

            string [] marketingUsernames = Roles.GetUsersInRole("Marketing");

            var marketer = from s in db.Staffs
                           from user in marketingUsernames
                           where s.UserName == user
                           select s;

            ViewBag.Marketer_List = new SelectList(marketer, "Staff_Id", "FirstName");

            return View();
        }

        //
        // POST: /Associate/Create

        [HttpPost]
        public ActionResult Create(Associate associate, FormCollection fc)
        {
            string password = fc["Password"];

            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name");
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name");
            
            string[] marketingUsernames = Roles.GetUsersInRole("Marketing");
            var marketer = from s in db.Staffs
                           from user in marketingUsernames
                           where s.UserName == user
                           select s;
            ViewBag.Marketer_List = new SelectList(marketer, "Staff_Id", "FirstName");
            
            if (password == null || password.Length < 6)
            {
                ModelState.AddModelError("AssociatePassword", "Password must have 6 characters."); 
            }

            //associate.Referrer = CookieHelper.getStaffId();
            
            if (ModelState.IsValid)
            {                
                db.Associates.AddObject(associate);

                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(associate.Username, password, associate.Email, null, null, true, null, out createStatus);
                Roles.AddUserToRole(associate.Username, "Associate");
               
                if (createStatus == MembershipCreateStatus.Success)
                {
                    db.SaveChanges();
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "New Associate Added");
                    return RedirectToAction("Index");
                }
                else
                {
                    NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Error Creating In New Associate");
                    return View(associate);
                }
                
               
            }

            return View(associate);
        }

        //
        // GET: /Associate/Edit/5

        public ActionResult Edit(int id)
        {
            Associate associate = db.Associates.Single(a => a.AssociateId == id);

            List<Address> l = new List<Address>();

            l.Add(associate.Address);

            //ViewBag.Country_Id = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName",associate.Address.Country.Country_Name);

            List<Country> c = new List<Country>();
            c.Add(associate.Address.Country);
            ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name",associate.Address.Country_Id);

            return View(associate);
        }

        //
        // POST: /Associate/Edit/5

        [HttpPost]
        public ActionResult Edit(Associate associate,FormCollection fc)
        {
            
            //ViewBag.Country_Id = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName");
            var country = fc["listCountry"];
            associate.Address.Country_Id = Convert.ToInt32(fc["listCountry"]);
            associate.Username = fc["Username"];
            associate.Referrer = Convert.ToInt32(fc["Referrer"]);
            if (ModelState.IsValid)
            {
                associate.Address.Address_Id =Convert.ToInt32(fc["Address_Id"]);
                Address address = associate.Address;
                //address.Country.Country_Id = Convert.ToInt32(fc["listCountry"]);
                
                //List<Country> c = new List<Country>();
                //c.Add(associate.Address.Country);
                //ViewBag.Country_Id = new SelectList(db.Countries, "Country_Id", "Country_Name");
                 //Country country = associate.Address.Country;
                associate.Address.Country = db.Countries.Single(x => x.Country_Id == associate.Address.Country_Id); 

                //db.Associates.Attach(associate);
                //db.ObjectStateManager.ChangeObjectState(address, EntityState.Modified); 
                db.ObjectStateManager.ChangeObjectState(associate, EntityState.Modified); 
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(associate);
        }

        //
        // GET: /Associate/Delete/5

        public ActionResult Delete(int id)
        {
            Associate associate = db.Associates.Single(a => a.AssociateId == id);
            return View(associate);
        }

        //
        // POST: /Associate/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Associate associate = db.Associates.Single(a => a.AssociateId == id);
            db.Associates.DeleteObject(associate);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }

    public class AssociateListHelper
    {
        public string AssociateName { get; set; }
        public int AssociateID { get; set; }

        public static IQueryable<AssociateListHelper> GetItems()
        {
            StormDBEntities db = new StormDBEntities();

            List<Associate> assoc = db.Associates.ToList();
            List<AssociateListHelper> assocHelper = new List<AssociateListHelper>();

            assocHelper.Add(new AssociateListHelper
            {
                AssociateName = "--None--",
                AssociateID = 0
            });

            foreach(Associate s in assoc)
            {
                assocHelper.Add( new AssociateListHelper {
                    AssociateName = s.AssociateName,
                    AssociateID = s.AssociateId
                });
            }

            return assocHelper.AsQueryable();
        }


    }
}