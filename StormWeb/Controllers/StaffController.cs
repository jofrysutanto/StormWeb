using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Models.ModelHelper;
using StormWeb.Helper;
using System.Web.Security;
using System.Web.Helpers;

namespace StormWeb.Controllers
{ 
    public class StaffController : Controller
    {
        private StormDBEntities db = new StormDBEntities();
        StormWeb.Helper.Enumclass Enumclass = new Enumclass();

        #region Index
        [Authorize(Roles = "Super,BranchManager")]
        public ViewResult Index()
        {
            var staffs = db.Staffs.Include("Address").Include("Staff_Dept");
            ViewBag.message = Request.QueryString["message"];
            return View(staffs.ToList());
        }

        #endregion

        #region MANAGE ROLE
        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult ManageRoles(int id)
        {
            ManageRoleModel roleModel = new ManageRoleModel();
            roleModel.rolesList = Roles.GetAllRoles();

            string staffUserName = db.Staffs.SingleOrDefault(x => x.Staff_Id == id).UserName;
            roleModel.assignedRolesList = Roles.GetRolesForUser(staffUserName);

            roleModel.username = staffUserName;

            return View(roleModel);
        }
      
        #endregion

        #region CHANGE ROLE
        [HttpPost]
        public ActionResult ChangeRole(string username, string role, string assign)
        {
            if (assign == "false")
            {
                Roles.RemoveUserFromRole(username, role);
            }
            else
            {
                Roles.AddUserToRole(username, role);
            }

            return Json(new { success = true });
        }

        #endregion

        #region Details
        [Authorize(Roles = "Super,BranchManager")]
        public ViewResult Details(int id)
        {
            Staff staff = db.Staffs.Single(s => s.Staff_Id == id);
            return View(staff);
        }

        #endregion 
        
        #region Create

        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult Create()
        {
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name");
            ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name");
            ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName");
            ViewBag.Branch = db.Branches.ToList();
            BindTitle("--Select--");
            Staff s = new Staff();

            s.Date_Of_Joining = DateTime.Now;

            return View(s);
        }  

        [HttpPost]
        public ActionResult Create(Staff staff, FormCollection fc)
        {
            if (staff.Dept_Id == 0)
            {
                ModelState.AddModelError("Dept_Id", "Please Select Your Department");
                ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName"); ViewBag.Branch = db.Branches.ToList();
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View(staff);
            }
            if (staff.Address.Address_Name == "" || staff.Address.Address_Name == null)
            {
                ModelState.AddModelError("Address_Name", "Please Select Your Address");
                ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName"); ViewBag.Branch = db.Branches.ToList();
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View(staff);
            }
            if (staff.Address.City == "" || staff.Address.City == null)
            {
                ModelState.AddModelError("City", "Please Select Your City");
                ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName"); ViewBag.Branch = db.Branches.ToList();
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View(staff);
            }
            if (staff.Address.State == "" || staff.Address.State == null)
            {
                ModelState.AddModelError("State", "Please Select Your State");
                ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName"); ViewBag.Branch = db.Branches.ToList();
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View(staff);
            }
            if (staff.Address.Zipcode == 0)
            {
                ModelState.AddModelError("Zipcode", "Please Select Your Zipcode");
                ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName"); ViewBag.Branch = db.Branches.ToList();
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View(staff);
            }
            if (staff.Address.Country_Id == 0 )
            {
                ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName"); ViewBag.Branch = db.Branches.ToList();
                ModelState.AddModelError("Country_Name", "Please Select Your Country Name");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View(staff);
            }
            if (staff.Title == "--Select--")
            {
                ModelState.AddModelError("Title", "Please Select Your Title");
                ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName"); ViewBag.Branch = db.Branches.ToList();
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View(staff);
            }
            if (staff.FirstName == "")
            {
                ModelState.AddModelError("FirstName", "Please Select Your FirstName");
                ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName"); ViewBag.Branch = db.Branches.ToList();
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View(staff);
            }
            if (staff.LastName == "")
            {
                ModelState.AddModelError("LastName", "Please Select Your LastName");
                ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName"); ViewBag.Branch = db.Branches.ToList();
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View(staff);
            }
            string username=fc["UserName"];
            var user = db.Staffs.Where(x => x.UserName == username).ToList();
            if (user.Count()!=0)
            { 
                ModelState.AddModelError("UserName", "UserName already Exists");
                ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName"); ViewBag.Branch = db.Branches.ToList();
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View(staff);
            }
            
             Branch_Staff bs = new Branch_Staff();

            if (fc["selectedBranch"] == String.Empty)
            {
                ModelState.AddModelError("selectedBranch", "Select one branch");
                BindTitle(staff.Title);
            }
            else
            {
                bs.Branch_Id = Convert.ToInt32(fc["selectedBranch"]);
                staff.Branch_Staff.Add(bs);
            }

            
            if (ModelState.IsValid)
            {
                db.Staffs.AddObject(staff);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }
            BindTitle(staff.Title);  
            ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName");
            ViewBag.Branch = db.Branches.ToList(); 
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name", staff.Address_Id);
            ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Created a new staff" + staff.FirstName + " " + staff.LastName ), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
            return View(staff);
        }
        
        #endregion

        #region Edit

        public ActionResult Edit(int id)
        {
            if (TempData[SUCCESS_EDIT] != null)
            {
                ViewBag.successEdit = true;
            }
            Staff staff = db.Staffs.Single(s => s.Staff_Id == id);
            
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name", staff.Address_Id);
            ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
            BindTitle(staff.Title);
            return View(staff);
        }
         
        [HttpPost]
        public ActionResult Edit(Staff staff)
        {
            if (staff.Dept_Id == 0)
            {
                ModelState.AddModelError("Dept_Id", "Please Select Your Department");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View();
            }
            if (staff.Address.Address_Name == "" || staff.Address.Address_Name==null)
            {
                ModelState.AddModelError("Address_Name", "Please Select Your Address");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View();
            }
            if (staff.Address.City == "" || staff.Address.City == null)
            {
                ModelState.AddModelError("City", "Please Select Your City");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View();
            } 
            if (staff.Address.State == "" || staff.Address.State == null)
            {
                ModelState.AddModelError("State", "Please Select Your State");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View();
            } 
            if (staff.Address.Zipcode == 0)
            {
                ModelState.AddModelError("Zipcode", "Please Select Your Zipcode");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View();
            }
            if (staff.Address.Country.Country_Name == "" || staff.Address.Country.Country_Name==null)
            {
                ModelState.AddModelError("Country_Name", "Please Select Your Country Name");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View();
            }
            if (staff.Title == "--Select--")
            {
                ModelState.AddModelError("Title", "Please Select Your Title");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View();
            }
            if (staff.FirstName == "")
            {
                ModelState.AddModelError("FirstName", "Please Select Your FirstName");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View();
            }
            if (staff.LastName == "")
            {
                ModelState.AddModelError("LastName", "Please Select Your LastName");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                BindTitle(staff.Title);
                return View();
            }
            //if (staff.DOB == "")
            //{
            //    ModelState.AddModelError("DOB", "Please Select Your DOB");
            //    return View();
            //} 
            //if (staff.Date_Of_Joining == "")
            //{
            //    ModelState.AddModelError("Date_Of_Joining", "Please Select Your Date Of Joining");
            //    return View();
            //}
            if (staff.Mobile_Number == "")
            {
                ModelState.AddModelError("Mobile_Number", "Please Select Your Mobile Number");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                return View();
            } 
            if (staff.Email == "")
            {
                ModelState.AddModelError("Email", "Please Select Your Email");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                return View();
            } 
            if (staff.UserName == "")
            {
                ModelState.AddModelError("UserName", "Please Select Your UserName");
                ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
                return View();
            }
            if (ModelState.IsValid)
            {
                Address address = staff.Address;
                address.Address_Id = staff.Address.Address_Id;
                address.Address_Name = staff.Address.Address_Name; 
                address.City=staff.Address.City;
                address.State=staff.Address.State;
                address.Zipcode=staff.Address.Zipcode;
                address.Country.Country_Name=staff.Address.Country.Country_Name;
                address.Country_Id = address.Country.Country_Id;
                
                Country country=staff.Address.Country;
                country.Country_Id=staff.Address.Country.Country_Id;
                country.Country_Name=staff.Address.Country.Country_Name;
                
               
                staff.Address.Country=country;
                staff.Address = address; 
                        
                staff.Address_Id = staff.Address.Address_Id;
               
                db.Staffs.Attach(staff);
                db.Addresses.Attach(address);
                db.Countries.Attach(country);
                db.ObjectStateManager.ChangeObjectState(staff, EntityState.Modified);
                db.ObjectStateManager.ChangeObjectState(address, EntityState.Modified);
                db.ObjectStateManager.ChangeObjectState(country, EntityState.Modified);
                db.SaveChanges();
                BindTitle(staff.Title);
                TempData[SUCCESS_EDIT] = true;
                return RedirectToAction("Edit", "Staff", new { message = "Successfully Edited" });
            }  
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name", staff.Address_Id);
            ViewBag.Dept_Id = new SelectList(db.Staff_Dept, "Dept_Id", "Dept_Name", staff.Dept_Id);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username +" Edited the Details of the staff " + staff.FirstName + " " + staff.LastName), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
            return View(staff);
        } 
        #endregion

        #region Delete

        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult Delete(int id)
        {
            Staff staff = db.Staffs.Single(s => s.Staff_Id == id);
            return View(staff);
        }

        [HttpPost, ActionName("Delete")] 
        public ActionResult DeleteConfirmed(int id)
        {            
            Staff staff = db.Staffs.Single(s => s.Staff_Id == id);
            List<Branch_Staff> branch_staffs = db.Branch_Staff.Where(s => s.Staff_Id == staff.Staff_Id).ToList();
            foreach (Branch_Staff bs in branch_staffs) db.Branch_Staff.DeleteObject(bs);
            var casestaff = db.Case_Staff.ToList().Where(s => s.Staff_Id == id);
            int a = casestaff.Count();
            if (casestaff.Count()!= 0)
            {
                ModelState.AddModelError("", "Cannot Delete .The Counsellor has cases assigned");
                return View();
            }  
            db.Staffs.DeleteObject(staff);
            db.SaveChanges();
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username+" Deleted the Details of the Staff" + staff.FirstName + " " + staff.LastName ), LogHelper.LOG_DELETE, LogHelper.SECTION_PROFILE);
            return RedirectToAction("Index");
        }
       
        #endregion

        private void BindTitle(string selectedValue)
        { 
            var title = Enumclass.GetTitle();
            ViewData["TitleValue"] = new SelectList(title, "Value", "Text", selectedValue.Trim());
        }
        
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        
        public static string SUCCESS_EDIT = "SuccessfulEdit";
    }
   
}       