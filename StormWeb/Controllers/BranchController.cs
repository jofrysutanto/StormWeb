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

        //To get the brancId to give edit and delete for the branch manager only to that specific branch
        public static int getBranchId()
        {
            StormDBEntities db = new StormDBEntities();
            int staffId = CookieHelper.getStaffId();
            var branchId = db.Branch_Staff.SingleOrDefault(x => x.Staff_Id == staffId).Branch_Id;
            return branchId;
        }
        public static string getBranchName()
        {
            StormDBEntities db = new StormDBEntities();
            int staffId = CookieHelper.getStaffId();
            var branchId = db.Branch_Staff.SingleOrDefault(x => x.Staff_Id == staffId).Branch_Id;
            string branchName = db.Branches.SingleOrDefault(x => x.Branch_Id == branchId).Branch_Name;
            return branchName;
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

                Address address = branches.Address;
                branches.Address = db.Addresses.Single(x => x.Address_Id == addressId);
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
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Edited the branch" + branches.Branch_Name);
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
            Branch branch = db.Branches.FirstOrDefault(x => x.Branch_Id == id);

            #region Getting the case id and deleting all details from tables Applications,Appointments,Case_Staff and CaseDocuments having that Case_Id

            var casesToDelete = db.Cases.Where(x => x.Branch_Id.Equals(branch.Branch_Id));

            var applicationsToDelete = (from applications in db.Applications
                                        from cases in db.Cases
                                        where cases.Case_Id == applications.Case_Id && cases.Branch_Id == branch.Branch_Id
                                        select applications);
            var appointmentToDelete = (from appointments in db.Appointments
                                       from cases in db.Cases
                                       where cases.Case_Id == appointments.Case_Id && cases.Branch_Id == branch.Branch_Id
                                       select appointments);
            var caseStaffToDelete = (from caseStaff in db.Case_Staff
                                     from cases in db.Cases
                                     where cases.Case_Id == caseStaff.Case_Id && cases.Branch_Id == branch.Branch_Id
                                     select caseStaff);

            var caseDocumentToDelete = (from caseDocuments in db.CaseDocuments
                                        from cases in db.Cases
                                        where cases.Case_Id == caseDocuments.Case_Id && cases.Branch_Id == branch.Branch_Id
                                        select caseDocuments);
            #endregion

            #region Deleting all the details from tables Branch_Staff that has the same Branch_Id

            var branchStaffToDelete = db.Branch_Staff.Where(x => x.Branch_Id.Equals(branch.Branch_Id));

            #endregion

            #region Getting the client id and deleting all details from tables Client_Children_Detail,Client_Qualification,Client_SkillTest,Client_Spouse,Client_StudyServiceDetails,Client_Work_Experience,Students and General_Enquiry having that Client_Id

            var clientToDelete = db.Clients.Where(x => x.Branch_Id.Equals(branch.Branch_Id));

            var clientChildrenToDelete = (from clientChildren in db.Client_Children_Detail
                                          from client in db.Clients
                                          where client.Client_Id == clientChildren.Client_Id && client.Branch_Id == branch.Branch_Id
                                          select clientChildren);

            var clientQualificationToDelete = (from clientQualification in db.Client_Qualification
                                               from client in db.Clients
                                               where client.Client_Id == clientQualification.Client_Id && client.Branch_Id == branch.Branch_Id
                                               select clientQualification);

            var clientSkillTestToDelete = (from clientSkillTest in db.Client_SkillTest
                                           from client in db.Clients
                                           where client.Client_Id == clientSkillTest.Client_Id && client.Branch_Id == branch.Branch_Id
                                           select clientSkillTest);

            var clientSpouseToDelete = (from clientSpouse in db.Client_Spouse
                                        from client in db.Clients
                                        where client.Client_Id == clientSpouse.Client_Id && client.Branch_Id == branch.Branch_Id
                                        select clientSpouse);

            var clientStudyServiceToDelete = (from clientStudyService in db.Client_StudyServiceDetails
                                              from client in db.Clients
                                              where client.Client_Id == clientStudyService.Client_Id && client.Branch_Id == branch.Branch_Id
                                              select clientStudyService);

            var clientWorkExperianceToDelete = (from clientWorkExperiance in db.Client_Work_Experience
                                                from client in db.Clients
                                                where client.Client_Id == clientWorkExperiance.Client_Id && client.Branch_Id == branch.Branch_Id
                                                select clientWorkExperiance);

            var studentToDelete = (from students in db.Students
                                   from client in db.Clients
                                   where client.Client_Id == students.Client_Id && client.Branch_Id == branch.Branch_Id
                                   select students);

            var generalEnquiryToDelete = (from generalEnquiry in db.General_Enquiry
                                          from client in db.Clients
                                          where client.Client_Id == generalEnquiry.Client_Id && client.Branch_Id == branch.Branch_Id
                                          select generalEnquiry);
            #endregion

            #region Getting the applicationId and deleting all the details from table that having the application Id

            var applicationsCancelToDelete = (from applicationCancel in db.Application_Cancel
                                              from applications in db.Applications
                                              from cases in db.Cases
                                              where cases.Case_Id == applications.Case_Id && cases.Branch_Id == branch.Branch_Id && applicationCancel.Application_Id==applications.Application_Id
                                              select applicationCancel);
            var applicationsDocumentToDelete = (from applicationDocument in db.Application_Document
                                              from applications in db.Applications
                                              from cases in db.Cases
                                              where cases.Case_Id == applications.Case_Id && cases.Branch_Id == branch.Branch_Id && applicationDocument.Application_Id == applications.Application_Id
                                              select applicationDocument);

            var applicationsResultToDelete = (from applicationResult in db.Application_Result
                                                from applications in db.Applications
                                                from cases in db.Cases
                                                where cases.Case_Id == applications.Case_Id && cases.Branch_Id == branch.Branch_Id && applicationResult.Application_Id == applications.Application_Id
                                                select applicationResult); 

            #endregion
          
            #region Deleting all details having the same Case_Id

            if (casesToDelete.Count() > 0)
            {
                foreach (Case cases in casesToDelete)
                {
                    db.Cases.DeleteObject(cases);
                }
            }

            if (applicationsToDelete.Count() > 0)
            {
                foreach (Application applications in applicationsToDelete)
                {
                    db.Applications.DeleteObject(applications);
                }
            }

            if (appointmentToDelete.Count() > 0)
            {
                foreach (Appointment appointments in appointmentToDelete)
                {
                    db.Appointments.DeleteObject(appointments);
                }
            }

            if (caseStaffToDelete.Count() > 0)
            {
                foreach (Case_Staff caseStaff in caseStaffToDelete)
                {
                    db.Case_Staff.DeleteObject(caseStaff);
                }
            }

            if (caseDocumentToDelete.Count() > 0)
            {
                foreach (CaseDocument caseDocument in caseDocumentToDelete)
                {
                    db.CaseDocuments.DeleteObject(caseDocument);
                }
            }

            #endregion

            #region Deleting all the details having the same Branch_Id

            if (branchStaffToDelete.Count() > 0)
            {
                foreach (Branch_Staff branchStaff in branchStaffToDelete)
                {
                    db.Branch_Staff.DeleteObject(branchStaff);
                }
            }

            #endregion

            #region Deleting all details having the same Client_Id

            if (clientToDelete.Count() > 0)
            {
                foreach (Client client in clientToDelete)
                {
                    db.Clients.DeleteObject(client);
                }
            }

            if (clientChildrenToDelete.Count() > 0)
            {
                foreach (Client_Children_Detail clientChildren in clientChildrenToDelete)
                {
                    db.Client_Children_Detail.DeleteObject(clientChildren);
                }
            }

            if (clientQualificationToDelete.Count() > 0)
            {
                foreach (Client_Qualification clientQualification in clientQualificationToDelete)
                {
                    db.Client_Qualification.DeleteObject(clientQualification);
                }
            }

            if (clientSkillTestToDelete.Count() > 0)
            {
                foreach (Client_SkillTest clientSkillTest in clientSkillTestToDelete)
                {
                    db.Client_SkillTest.DeleteObject(clientSkillTest);
                }
            }

            if (clientSpouseToDelete.Count() > 0)
            {
                foreach (Client_Spouse clientSpouse in clientSpouseToDelete)
                {
                    db.Client_Spouse.DeleteObject(clientSpouse);
                }
            }

            if (clientStudyServiceToDelete.Count() > 0)
            {
                foreach (Client_StudyServiceDetails clientStudyService in clientStudyServiceToDelete)
                {
                    db.Client_StudyServiceDetails.DeleteObject(clientStudyService);
                }
            }
            if (clientWorkExperianceToDelete.Count() > 0)
            {
                foreach (Client_Work_Experience clientWorkExperiance in clientWorkExperianceToDelete)
                {
                    db.Client_Work_Experience.DeleteObject(clientWorkExperiance);
                }
            }


            if (studentToDelete.Count() > 0)
            {
                foreach (Student students in studentToDelete)
                {
                    db.Students.DeleteObject(students);
                }
            }

            if (generalEnquiryToDelete.Count() > 0)
            {
                foreach (General_Enquiry generalEnquiry in generalEnquiryToDelete)
                {
                    db.General_Enquiry.DeleteObject(generalEnquiry);
                }
            }


            #endregion

            #region Deleting all details having the same Application_Id

            if (applicationsCancelToDelete.Count() > 0)
            {
                foreach (Application_Cancel applicationCancel in applicationsCancelToDelete)
                {
                    db.Application_Cancel.DeleteObject(applicationCancel);
                }
            }

            if (applicationsDocumentToDelete.Count() > 0)
            {
                foreach (Application_Document applicationDocument in applicationsDocumentToDelete)
                {
                    db.Application_Document.DeleteObject(applicationDocument);
                }
            }

            if (applicationsResultToDelete.Count() > 0)
            {
                foreach (Application_Result applicationResult in applicationsResultToDelete)
                {
                    db.Application_Result.DeleteObject(applicationResult);
                }
            }

            #endregion

            db.Branches.DeleteObject(branch);
            db.SaveChanges();
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully deleted the Branch " + branch.Branch_Name );
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