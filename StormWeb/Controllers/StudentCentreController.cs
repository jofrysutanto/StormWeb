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
    public class StudentCentreController : Controller
    {
        private StormDBEntities db = new StormDBEntities();
        StormWeb.Helper.Enumclass Enumclass = new Enumclass();

        #region Student Home Page
        [Authorize(Roles = "Super,student,Counsellor")]
        public ActionResult Student()
        {
            ViewBag.eve = db.Events.Where(x => x.Date > DateTime.Now).ToList();
            StudentCentreModel stmodel = new StudentCentreModel();
            int studentId = 0;
            if (CookieHelper.isStudent())
            {
                studentId = Convert.ToInt32(CookieHelper.StudentId);

                #region Progress Details

                ViewBag.id = studentId;
                var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);
                stmodel.caseTable = cases.ToList();
                var course = from app in db.Applications
                             from co in db.Courses
                             where co.Course_Id == app.Course_Id && app.Student_Id == studentId
                             select co;
                ViewBag.applicationCount = course.Count();
                stmodel.courseTable = course.ToList();

                #endregion

                #region Counsellor Details

                String counsellorName = "";
                foreach (var item in cases)
                {
                    if (item.Case_Staff.Count != 0)
                    {
                        var name = item.Case_Staff.Where(x => x.Role == "Counsellor").Where(x => x.Case_Id == item.Case_Id);
                        //counsellorName = item.Case_Staff.First().Staff.FirstName + " " + item.Case_Staff.First().Staff.LastName;
                        counsellorName = name.First().Staff.FirstName + " " + name.First().Staff.LastName;
                        ViewBag.contact = item.Case_Staff.First().Staff.Mobile_Number;
                        ViewBag.email = item.Case_Staff.First().Staff.Email;
                        ViewBag.branch = item.Branch.Branch_Name;
                    }
                }
                if (counsellorName == "")
                    ViewBag.name = "You do not have a counsellor assigned";
                else
                    ViewBag.name = counsellorName;

                #endregion

                #region Get the Appointment Details

                DateTime current = DateTime.Now;
                StormDBEntities countDB = new StormDBEntities();
                var upcomingAppointment = "";
                var upcomingApp = from app in countDB.Appointments
                                  where app.Confirmation == "Confirmed" && app.Case.Student_Id == studentId && app.AppDateTime >= current
                                  select app;
                if (upcomingApp == null || upcomingApp.Count() <= 0)
                    upcomingAppointment = "0";
                else
                    upcomingAppointment = upcomingApp.Count().ToString();

                ViewBag.upcomingAppointment = upcomingAppointment;

                #endregion

                #region Get New Message

                StormDBEntities dbCon = new StormDBEntities();

                string username = CookieHelper.Username;

                int documentcount = (from mt in dbCon.Message_To
                                     from m in dbCon.Messages
                                     where m.Id == mt.Message_Id && mt.UserTo == username && mt.HasRead == false && mt.Deleted == false
                                     select mt).ToList().Count;
                ViewBag.count = documentcount;
                #endregion

                #region Document List

                var caseDocumentTemplate = db.CaseDoc_Template.ToList();
                var application = db.Applications.ToList().Where(x => x.Student_Id == studentId);

                var caseDocuments = from caseTable in db.Cases
                                    from caseDocumentTable in db.CaseDocuments
                                    where caseTable.Student_Id == studentId && caseDocumentTable.Case_Id == caseTable.Case_Id
                                    select caseDocumentTable;
                var applicationDocuments = from app in db.Applications
                                           from doc in db.Application_Document
                                           where app.Application_Id == doc.Application_Id && app.Student_Id == studentId
                                           select doc;

                ViewBag.caseDocumentCount = caseDocuments.Count();
                ViewBag.applicationListcount = applicationDocuments.ToList();
                ViewBag.totalCount = caseDocumentTemplate.Count() + application.Count();


                #endregion

                # region Student Details

                var student = db.Students.ToList().Where(x => x.Student_Id == studentId);
                var client = db.Clients.ToList().Where(x => x.Client_Id == student.First().Client_Id);
                stmodel.clientTable = client.ToList();

                #endregion

                ViewBag.Ads = db.Advertisements.Where(x => x.ExpiryDate > DateTime.Now && x.Audience.Contains("student")).ToList();

                return View(stmodel);
            }
            return View();
        }

        #endregion

        public ActionResult CounsellorContactDetails()
        {
            int studentId = 0;
            studentId = Convert.ToInt32(CookieHelper.StudentId);
            var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);
            String counsellorName = "";
            foreach (var item in cases)
            {
                if (item.Case_Staff.Count != 0)
                {
                    var name = item.Case_Staff.Where(x => x.Role == "Counsellor").Where(x => x.Case_Id == item.Case_Id);
                    //counsellorName = item.Case_Staff.First().Staff.FirstName + " " + item.Case_Staff.First().Staff.LastName;
                    counsellorName = name.First().Staff.FirstName + " " + name.First().Staff.LastName;
                    ViewBag.contact = item.Case_Staff.First().Staff.Mobile_Number;
                    ViewBag.email = item.Case_Staff.First().Staff.Email;
                    ViewBag.branch = item.Branch.Branch_Name;
                }
            }
            if (counsellorName == "")
                ViewBag.name = "You do not have a counsellor assigned";
            else
                ViewBag.name = counsellorName;
            return View();
        }

        #region List Student Profile
        [Authorize(Roles = "Super,student,Counsellor")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Profile(int id)
        {
            int studentId = id;
            //Session["studentId"] = id; 
            ViewBag.id = id;
            ViewBag.message = Request.QueryString["message"];

            StudentCentreModel stmodel = new StudentCentreModel();
            //int studentId = Convert.ToInt32(CookieHelper.StudentId);
            var client = db.Students.Single(c => c.Student_Id == studentId);
            var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);
            stmodel.caseTable = cases.ToList();

            stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            //ViewBag.Message = Request.QueryString["message"];
            return View(stmodel);
        }

        #endregion

        #region Add

        #region Add Qualification
        public ActionResult AddQualification(int id)
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddQualification(int id, Client_Qualification clientQualification)
        {
            if (ModelState.IsValid)
            {
                StudentCentreModel stmodel = new StudentCentreModel();
                int studentId = id;
                var student = db.Students.Single(c => c.Student_Id == studentId);
                var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);

                clientQualification.Client_Id = student.Client_Id;
                stmodel.caseTable = cases.ToList();
                stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
                stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
                stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
                stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
                stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
                stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();

                if (clientQualification.Qualification == null || clientQualification.Qualification == "")
                {
                    ModelState.AddModelError("Qualification", "Enter Your Qualification");
                    return View(stmodel);
                }
                if (clientQualification.Major == null || clientQualification.Major == "")
                {
                    ModelState.AddModelError("Major", "Enter Your Major");
                    return View(stmodel);
                }
                if (clientQualification.University == null || clientQualification.University == "")
                {
                    ModelState.AddModelError("University", "Enter Your University");
                    return View(stmodel);
                }

                if (clientQualification.Year_of_Passing == 0)
                {
                    ModelState.AddModelError("Year_of_Passing", "Enter Your Year Of Passing");
                    return View(stmodel);
                }
                db.Client_Qualification.AddObject(clientQualification);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username +" Added Qualification to Student Profile of the Student " + student.Client.GivenName + " " + student.Client.LastName), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Added Qualification");
                return View("Refresh");
            }
            return View("Refresh");
        }
        #endregion

        #region Add Children Details
        public ActionResult AddChildrensDetails(int id)
        {
            BindGender("--Select--");
            return View();
        }
        [HttpPost]
        public ActionResult AddChildrensDetails(int id, Client_Children_Detail childrenDetails)
        {
            if (ModelState.IsValid)
            {
                StudentCentreModel stmodel = new StudentCentreModel();
                int studentId = id;
                var student = db.Students.Single(c => c.Student_Id == studentId);
                int clientId = student.Client_Id;
                Case cases = new Case();
                cases = db.Cases.Single(x => x.Student_Id == studentId);

                string str = childrenDetails.Gender;
                childrenDetails.Client_Id = clientId;

                if (childrenDetails.Given_Name == "" || childrenDetails.Given_Name == null)
                {

                    ModelState.AddModelError("givenName", "Enter Your Given Name");
                    return View(stmodel);
                }
                if (childrenDetails.Sur_Name == "" || childrenDetails.Sur_Name == null)
                {
                    ModelState.AddModelError("surName", "Enter Your Surname");
                    return View(stmodel);
                }
                if (childrenDetails.DOB.ToString() == "0/0/0001")
                {
                    ModelState.AddModelError("givenName", "Enter Your Date of Birth");
                    return View(stmodel);
                }
                if (childrenDetails.Gender == "--Select--")
                {
                    ModelState.AddModelError("Gender", "Enter Your Gender");
                    return View(stmodel);
                }

                db.Client_Children_Detail.AddObject(childrenDetails);
                db.SaveChanges();


                stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == clientId).ToList();
                BindGender(childrenDetails.Gender);

                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added Childrens Details to the Student Profile of the Student" + student.Client.GivenName + " " + student.Client.LastName ), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Added Children's Details");

                return View("Refresh");
            }
            return View("Refresh");
        }
        #endregion

        #region Add Skill Test Details
        public ActionResult AddSkillTest(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddSkillTest(int id, Client_SkillTest skills)
        {
            if (ModelState.IsValid)
            {
                StudentCentreModel stmodel = new StudentCentreModel();
                int studentId = id;
                var student = db.Students.Single(c => c.Student_Id == studentId);
                int clientId = student.Client_Id;
                skills.Client_Id = clientId;
                Case cases = new Case();
                cases = db.Cases.Single(x => x.Student_Id == studentId);

                stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == clientId).ToList();


                db.Client_SkillTest.AddObject(skills);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added Skills to  the Student Profile of the Student " + student.Client.GivenName + " " + student.Client.LastName ), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Added Skills");

                return View("Refresh");
            }
            return View("Refresh");
        }
        #endregion

        #region Add Spouse Details
        public ActionResult AddSpouseDetails(int id)
        {
            BindTitle("--Select--");
            return View();
        }

        [HttpPost]
        public ActionResult AddSpouseDetails(int id, Client_Spouse spouse)
        {
            if (ModelState.IsValid)
            {
                StudentCentreModel stmodel = new StudentCentreModel();
                int studentId = id;
                Case cases = new Case();
                cases = db.Cases.Single(x => x.Student_Id == studentId);
                var student = db.Students.Single(c => c.Student_Id == studentId);
                int clientId = student.Client_Id;

                spouse.Client_Id = clientId;

                stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == clientId).ToList();
                BindTitle(spouse.Title);

                if (spouse.Title == "--Select--")
                {
                    ModelState.AddModelError("title", "Enter Select Your Title");
                    return View(stmodel);
                }
                if (spouse.GivenName == "" || spouse.GivenName == null)
                {
                    ModelState.AddModelError("givenName", "Enter Your Given Name");
                    return View(stmodel);
                }
                if (spouse.LastName == "" || spouse.LastName == null)
                {
                    ModelState.AddModelError("lastName", "Enter Your Surname");
                    return View(stmodel);
                }
                if (spouse.PrimaryContact == "" || spouse.PrimaryContact == null)
                {
                    ModelState.AddModelError("primaryContact", "Enter Your Primary Contact");
                    return View(stmodel);
                }

                db.Client_Spouse.AddObject(spouse);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added Spouse Details to the  Student Profile of the Student" + student.Client.GivenName + " " + student.Client.LastName ), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Added Spouse Details");
                return View("Refresh");
            } return View("Refresh");
        }
        #endregion

        #region Add Study Service Details
        public ActionResult AddStudyServiceDetails()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddStudyServiceDetails(int id,Client_StudyServiceDetails services)
        {
            if (ModelState.IsValid)
            {
                StudentCentreModel stmodel = new StudentCentreModel();
                // int studentId = Convert.ToInt32(CookieHelper.StudentId);
                int studentId = id;
                Case cases = new Case();
                cases = db.Cases.Single(x => x.Student_Id == studentId);

                var student = db.Students.Single(c => c.Student_Id == studentId);
                int clientId = student.Client_Id;
                 
                stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == clientId).ToList();

                if (services.HearAboutUs == "")
                {
                    ModelState.AddModelError("title", "Please tell us how did you hear about us");
                    return View(stmodel);
                }

                services.Client_Id = clientId;
                db.Client_StudyServiceDetails.AddObject(services);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added Study Service Details to the Student Profile of the Student " + student.Client.GivenName + " " + student.Client.LastName), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Added Service Details");
                return View("Refresh"); 
            } return View("Refresh");
        }
        #endregion

        #region Add Work Experiance
        public ActionResult AddWorkExperiance(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddWorkExperiance(int id, Client_Work_Experience workExperiance)
        {
            if (ModelState.IsValid)
            {

                //int studentId = Convert.ToInt32(CookieHelper.StudentId);
                StudentCentreModel stmodel = new StudentCentreModel();
                int studentId = id;
                var student = db.Students.Single(c => c.Student_Id == studentId);
                int clientId = student.Client_Id;

                Case cases = new Case();
                cases = db.Cases.Single(x => x.Student_Id == studentId);

                workExperiance.Client_Id = clientId;

                stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == clientId).ToList();

                if (workExperiance.From_Date.ToString() == "0/01/0001")
                {
                    ModelState.AddModelError("From_Date", "Please Enter the From Date");
                    return View(stmodel);
                }
                if (workExperiance.To_Date.ToString() == "0/01/0001")
                {
                    ModelState.AddModelError("To_Date", "Please Enter the To Date");
                    return View(stmodel);
                }
                if (workExperiance.Country == "" || workExperiance.Country == null)
                {
                    ModelState.AddModelError("To_Date", "Please Enter the Country Name");
                    return View(stmodel);
                }
                if (workExperiance.Employer == "" || workExperiance.Employer == null)
                {
                    ModelState.AddModelError("To_Date", "Please Enter the Employer Name");
                    return View(stmodel);
                }
                if (workExperiance.Position == "" || workExperiance.Position == null)
                {
                    ModelState.AddModelError("To_Date", "Please Enter your position");
                    return View(stmodel);
                }
                if (workExperiance.Duties == "" || workExperiance.Duties == null)
                {
                    ModelState.AddModelError("To_Date", "Please Enter your duties");
                    return View(stmodel);
                }
                db.Client_Work_Experience.AddObject(workExperiance);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added Work Experiance to the Student Profile of the Student " + student.Client.GivenName + " " + student.Client.LastName ), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Added Work Experiance");
                return View("Refresh");
            }
            return View("Refresh");
        }
        #endregion

        #endregion

        #region Edit

        #region Edit Student Profile
        [Authorize(Roles = "Super,student,Counsellor")]
        public ActionResult EditStudentProfile(int id)
        {
        //    if (TempData[SUCCESS_EDIT] != null)
        //    {
        //        ViewBag.successEdit = true;
        //    }
            StudentCentreModel stmodel = new StudentCentreModel();
            //int studentId = Convert.ToInt32(CookieHelper.StudentId);
            int studentId = id;
            var student = db.Students.Single(c => c.Student_Id == studentId);
            var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);
            var client = db.Clients.Single(x => x.Client_Id == student.Client_Id);

            stmodel.caseTable = cases.ToList();

            stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            BindTitle(client.Title);
            BindMaritalStatus(client.MaritalStatus);
             
            return View(stmodel);
        }

        [HttpPost]
        public ActionResult EditStudentProfile(StudentCentreModel stmodel, FormCollection collection, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Case cases = new Case();

                    int caseId = Convert.ToInt32(collection["item.Case_Id"]);
                    cases = db.Cases.Single(x => x.Case_Id == caseId);

                    Client client = cases.Student.Client;

                    Address address = cases.Student.Client.Address;
                    Student student = cases.Student;
                    Country country = cases.Student.Client.Address.Country;

                    student.UserName = collection["item.Student.UserName"];
                    student.Course_Choice = collection["item.Student.Course_Choice"];

                    client.Client_Id = Convert.ToInt32(collection["item.Student.Client.Client_Id"]);
                    client.Title = collection["item.Student.Client.Title"];
                    client.Dob = Convert.ToDateTime(collection["item.Student.Client.Dob"]);
                    client.GivenName = collection["givenName"];
                    client.LastName = collection["lastName"];
                    client.Email = collection["email"];
                    client.ContactNumber = collection["contactNumber"];
                    client.SecondaryContactNumber = collection["secondaryContactNumber"];
                    client.Services = collection["item.Student.Client.Services"];
                    client.PreferredCountry = collection["preferredCountry"];
                    client.InPreferredCountry = Convert.ToBoolean(collection["inPreferredCountry"]);
                    client.PreferredCountryVisaStatus = collection["preferredCountryVisaStatus"];
                    client.MaritalStatus = collection["item.Student.Client.MaritalStatus"];
                    client.RelatedKinDetail = collection["relatedKinDetail"];
                    client.Nationality = collection["item.Student.Client.Nationality"];
                    client.Registered_On = Convert.ToDateTime(collection["item.Student.Client.Registered_On.Value"]);

                    BindMaritalStatus(client.MaritalStatus);
                    BindTitle(client.Title);

                    student.Client_Id = client.Client_Id;

                    address.Address_Name = collection["addressName"];
                    address.City = collection["city"];
                    address.State = collection["state"];
                    address.Zipcode = Convert.ToInt32(collection["zipCode"]);

                    cases.Case_Id = Convert.ToInt32(collection["item.Case_Id"]);
                    cases.Branch_Id = Convert.ToInt32(collection["item.Branch_Id"]);
                    cases.Student = student;
                    cases.Student.Client = client;
                    cases.Student.Client.Address = address;
                    cases.Student_Id = student.Student_Id;

                    stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                    stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
                    stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
                    stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
                    stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
                    stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
                    stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();

                    if (collection["item.Student.Client.Title"] == "--Select--")
                    {
                        ModelState.AddModelError("Title", "Please Select Your Title");
                        return View(stmodel);
                    }
                    if (collection["givenName"] == "")
                    {
                        ModelState.AddModelError("givenName", "Please Enter Your Given Name");
                        return View(stmodel);
                    }
                    if (collection["lastName"] == "")
                    {
                        ModelState.AddModelError("lastName", "Please Enter Your Last Name");
                        return View(stmodel);
                    }
                    if (collection["contactNumber"] == "")
                    {
                        ModelState.AddModelError("contactNumber", "Please Enter Your Contact Number");
                        return View(stmodel);
                    }
                    if (collection["item.Student.Client.Services"] == "")
                    {
                        ModelState.AddModelError("Services", "Please Enter Your Services");
                        return View(stmodel);
                    }
                    if (collection["email"] == "")
                    {
                        ModelState.AddModelError("lastName", "Please Enter Your Email");
                        return View(stmodel);
                    }


                    db.ObjectStateManager.ChangeObjectState(cases, EntityState.Modified);

                    db.SaveChanges();

                    //TempData[SUCCESS_EDIT] = true;

                    LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username+" Edited Primary Details of the Student " + student.Client.GivenName + " " + student.Client.LastName ), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Edited Student Profile");
                    return View("Refresh");
                    // return View(stmodel);
                   // return RedirectToAction("EditStudentProfile", "StudentCentre", new { message = "Successfully Edited" });
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} First exception caught.", e);
                Debug.WriteLine(e.InnerException);
                ModelState.AddModelError("", e);
            }
            return View(stmodel);
        }

        #endregion

        #region EditQualification
        [Authorize(Roles = "Super,student,Counsellor")]
        public ActionResult EditQualification(int id, int qualificationId)
        {
            StudentCentreModel stmodel = new StudentCentreModel();
            //int studentId = Convert.ToInt32(CookieHelper.StudentId);
            int studentId = id;
            var client = db.Students.Single(c => c.Student_Id == studentId);
            var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);
            stmodel.caseTable = cases.ToList();

            stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Id == qualificationId).ToList();
            stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();

            return View(stmodel);
        }
        [HttpPost]
        public ActionResult EditQualification(StudentCentreModel stmodel, FormCollection collection, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int studentId = id;
                    int qualificationId = Convert.ToInt32(collection["item.Id"]);
                    Case cases = new Case();
                    cases = db.Cases.Single(x => x.Student_Id == studentId);

                    int clientId = Convert.ToInt32(collection["item.Client_Id"]);

                    foreach (var item in cases.Student.Client.Client_Qualification.Where(x => x.Id == qualificationId))
                    {
                        item.Qualification = collection["qualification"];
                        item.Major = collection["major"];
                        item.University = collection["university"];
                        item.Year_of_Passing = Convert.ToInt32(collection["yearOfPassing"]);
                    }
                    stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                    stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == clientId).ToList();

                    if (collection["qualification"] == "")
                    {
                        ModelState.AddModelError("Qualification", "Enter Your Qualification");
                        return View(stmodel);
                    }
                    if (collection["major"] == "")
                    {
                        ModelState.AddModelError("Major", "Enter Your Major");
                        return View(stmodel);
                    }
                    if (collection["university"] == "")
                    {
                        ModelState.AddModelError("University", "Enter Your University");
                        return View(stmodel);
                    }

                    if (collection["yearOfPassing"] == "")
                    {
                        ModelState.AddModelError("Year_of_Passing", "Enter Your Year Of Passing");
                        return View(stmodel);
                    }
                    db.ObjectStateManager.ChangeObjectState(cases, EntityState.Modified);

                    db.SaveChanges();
                    LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username+" Edited the Qualification of the Student " + cases.Student.Client.GivenName + " " + cases.Student.Client.LastName), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully edited Qualification");
                    return View("Refresh");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} First exception caught.", e);
                Debug.WriteLine(e.InnerException);
                ModelState.AddModelError("", e);
            }
            return View(stmodel);
        }
        #endregion

        #region Edit Childrens Details
        [Authorize(Roles = "Super,student,Counsellor")]
        public ActionResult EditChildrensDetails(int id, int childId)
        {
            //if (TempData[SUCCESS_EDIT] != null)
            //{
            //    ViewBag.successEdit = true;
            //}

            StudentCentreModel stmodel = new StudentCentreModel();
            //int studentId = Convert.ToInt32(CookieHelper.StudentId);
            int studentId = id;
            var student = db.Students.Single(c => c.Student_Id == studentId);
            var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);
            stmodel.caseTable = cases.ToList();

            stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Id == childId).ToList();
            stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            ViewBag.clientId = student.Client_Id;
            if (stmodel.clientChildrenDetail.Count != 0)
                BindGender(stmodel.clientChildrenDetail.First().Gender.TrimEnd());
            return View(stmodel);
        }

        [HttpPost]
        public ActionResult EditChildrensDetails(StudentCentreModel stmodel, FormCollection collection, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int childId = Convert.ToInt32(collection["item.Id"]);
                    //int studentId = Convert.ToInt32(CookieHelper.StudentId);
                    int studentId = id;
                    Case cases = new Case();
                    cases = db.Cases.Single(x => x.Student_Id == studentId);

                    int clientId = Convert.ToInt32(collection["item.Client_Id"]);
                    //int id = Convert.ToInt32(collection["item.Id"]);
                    string str = collection["item.Gender"];
                    foreach (var item in cases.Student.Client.Client_Children_Detail.Where(x => x.Id == childId))
                    {
                        item.Given_Name = collection["givenName"];
                        item.Sur_Name = collection["surName"];
                        item.DOB = Convert.ToDateTime(collection["item.DOB"]);
                        item.Gender = collection["item.Gender"];
                        item.Notes = collection["Notes"];

                    }
                    stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                    stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == clientId).ToList();
                    BindGender(collection["item.Gender"]);
                    if (collection["givenName"] == "")
                    {

                        ModelState.AddModelError("givenName", "Enter Your Given Name");
                        // return View(cases);
                        return View(stmodel);
                    }
                    if (collection["surName"] == "")
                    {
                        ModelState.AddModelError("surName", "Enter Your Surname");
                        return View(stmodel);
                    }
                    if (collection["item.DOB"] == "")
                    {
                        ModelState.AddModelError("givenName", "Enter Your Date of Birth");
                        return View(stmodel);
                    }
                    if (collection["item.Gender"] == "--Select--")
                    {
                        ModelState.AddModelError("Gender", "Enter Your Gender");
                        return View(stmodel);
                    }
                    db.ObjectStateManager.ChangeObjectState(cases, EntityState.Modified);
                    db.SaveChanges();
                   // TempData[SUCCESS_EDIT] = true;
                    LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited Children's Details of Student" + cases.Student.Client.GivenName + " " + cases.Student.Client.LastName), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Edited Childrens Details");
                    return View("Refresh");
                    //return View(stmodel);
                    //return RedirectToAction("EditChildrensDetails", "StudentCentre", new { childId = childId, message = "Successfully edited" });

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} First exception caught.", e);
                Debug.WriteLine(e.InnerException);
                ModelState.AddModelError("", e);
            }
            return View(stmodel);
        }
        #endregion

        #region Edit Skill Test
        [Authorize(Roles = "Super,student,Counsellor")]
        public ActionResult EditSkillTest(int id, int skillsId)
        {
            //if (TempData[SUCCESS_EDIT] != null)
            //{
            //    ViewBag.successEdit = true;
            //}
            StudentCentreModel stmodel = new StudentCentreModel();
            //int studentId = Convert.ToInt32(CookieHelper.StudentId);
            int studentId = id;
            var client = db.Students.Single(c => c.Student_Id == studentId);
            var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);
            stmodel.caseTable = cases.ToList();

            stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_SkillTestId == skillsId).ToList();
            stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            ViewBag.clientId = client.Client_Id;
            return View(stmodel);
        }

        [HttpPost]
        public ActionResult EditSkillTest(StudentCentreModel stmodel, FormCollection collection, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int studentId = id;
                    Case cases = new Case();
                    cases = db.Cases.Single(x => x.Student_Id == studentId);

                    int clientId = Convert.ToInt32(collection["item.Client_Id"]);
                    int skillsid = Convert.ToInt32(collection["item.Client_SkillTestId"]);


                    stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                    stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == clientId).ToList();

                    foreach (var item in cases.Student.Client.Client_SkillTest.Where(x => x.Client_SkillTestId == skillsid))
                    {
                        item.TypeOfTest = collection["typeOfTest"];
                        item.Score = collection["score"];

                    }

                    db.ObjectStateManager.ChangeObjectState(cases, EntityState.Modified);
                    db.SaveChanges();
                    //  TempData[SUCCESS_EDIT] = true;
                    //return View(stmodel);
                    //return RedirectToAction("EditSkillTest", "StudentCentre", new { message = "Successfully edited" });
                    LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited Skill test Details of the student" + cases.Student.Client.GivenName + " " + cases.Student.Client.LastName ), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Edited Skills");
                    return View("Refresh");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} First exception caught.", e);
                Debug.WriteLine(e.InnerException);
                ModelState.AddModelError("", e);
            }
            return View(stmodel);
        }

        #endregion

        #region Edit Spouse Details
        [Authorize(Roles = "Super,student,Counsellor")]
        public ActionResult EditSpouseDetails(int id, int spouseId)
        {
            //if (TempData[SUCCESS_EDIT] != null)
            //{
            //    ViewBag.successEdit = true;
            //}
            StudentCentreModel stmodel = new StudentCentreModel();
            //int studentId = Convert.ToInt32(CookieHelper.StudentId);
            int studentId = id;
            var student = db.Students.Single(c => c.Student_Id == studentId);
            var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);
            stmodel.caseTable = cases.ToList();

            stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Id == spouseId).ToList();
            stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == student.Client_Id).ToList();
            ViewBag.clientId = student.Client_Id;
            BindTitle(student.Client.Client_Spouse.First().Title.TrimEnd());
            return View(stmodel);
        }

        [HttpPost]
        public ActionResult EditSpouseDetails(StudentCentreModel stmodel, FormCollection collection, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //int studentId = Convert.ToInt32(CookieHelper.StudentId);
                    int studentId = id;
                    Case cases = new Case();
                    cases = db.Cases.Single(x => x.Student_Id == studentId);
                    int clientId = Convert.ToInt32(collection["item.Client_Id"]);
                    int spouseId = Convert.ToInt32(collection["item.Id"]);
                     
                    stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                    stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == clientId).ToList();
                    BindTitle(collection["item.Title"]);

                    if (collection["item.title"] == "--Select--")
                    {
                        ModelState.AddModelError("title", "Enter Select Your Title");
                        return View(stmodel);
                    }
                    if (collection["givenName"] == "")
                    {
                        ModelState.AddModelError("givenName", "Enter Your Given Name");
                        return View(stmodel);
                    }
                    if (collection["lastName"] == "")
                    {
                        ModelState.AddModelError("lastName", "Enter Your Surname");
                        return View(stmodel);
                    }
                    if (collection["primaryContact"] == "")
                    {
                        ModelState.AddModelError("primaryContact", "Enter Your Primary Contact");
                        return View(stmodel);
                    }

                    foreach (var item in cases.Student.Client.Client_Spouse.Where(x => x.Id == spouseId))
                    {
                        item.Title = collection["item.Title"];
                        item.GivenName = collection["givenName"];
                        item.LastName = collection["lastName"]; 
                        item.PrimaryContact = collection["primaryContact"];
                        item.Dob = Convert.ToDateTime(collection["item.Dob"]);
                        item.Nationality = collection["nationality"];
                    }
                    db.ObjectStateManager.ChangeObjectState(cases, EntityState.Modified);

                    db.SaveChanges();
                    LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited Spouse Details of the Student " + cases.Student.Client.GivenName + " " + cases.Student.Client.LastName), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Edited Spouse Details");
                    return View("Refresh");
                    // TempData[SUCCESS_EDIT] = true;
                    // return View(stmodel);
                    // return RedirectToAction("EditSpouseDetails", "StudentCentre", new { message = "Successfully Edited" });
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} First exception caught.", e);
                Debug.WriteLine(e.InnerException);
                ModelState.AddModelError("", e);
            }
            return View(stmodel);
        }

        #endregion

        #region Edit Study Service Details
        [Authorize(Roles = "Super,student,Counsellor")]
        public ActionResult EditStudyServiceDetails(int id)
        {
            //if (TempData[SUCCESS_EDIT] != null)
            //{
            //    ViewBag.successEdit = true;
            //}
            StudentCentreModel stmodel = new StudentCentreModel();
            //int studentId = Convert.ToInt32(CookieHelper.StudentId);
            int studentId = id;
            var client = db.Students.Single(c => c.Student_Id == studentId);
            var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);
            stmodel.caseTable = cases.ToList();

            stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            ViewBag.clientId = client.Client_Id;
            return View(stmodel);
        }

        [HttpPost]
        public ActionResult EditStudyServiceDetails(StudentCentreModel stmodel, FormCollection collection, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    // int studentId = Convert.ToInt32(CookieHelper.StudentId);
                    int studentId = id;
                    Case cases = new Case();
                    cases = db.Cases.Single(x => x.Student_Id == studentId);

                    int clientId = Convert.ToInt32(collection["item.Client_Id"]);
                    bool relativeAtPreferredCountry = false;
                    string a = collection["item.RelativeAtPreferredCountry"];
                    if (a == "true,false")
                        relativeAtPreferredCountry = true;
                    else
                        relativeAtPreferredCountry = false;
                    cases.Student.Client.Client_StudyServiceDetails.First().PreferredCourseDetail = collection["preferredCourseDetail"];
                    cases.Student.Client.Client_StudyServiceDetails.First().AppliedToOtherUniversity = Boolean.Parse(collection["item.AppliedToOtherUniversity"]);
                    cases.Student.Client.Client_StudyServiceDetails.First().AppliedToOtherUniversityName = collection["appliedToOtherUniversityName"];
                    cases.Student.Client.Client_StudyServiceDetails.First().PreviousConsultant = collection["previousConsultant"];
                    cases.Student.Client.Client_StudyServiceDetails.First().RelativeAtPreferredCountry = relativeAtPreferredCountry;
                    cases.Student.Client.Client_StudyServiceDetails.First().RelativeAtPreferredCountryLocation = collection["relativeAtPreferredCountryLocation"];
                    cases.Student.Client.Client_StudyServiceDetails.First().RelativeAtPreferredCountyStatus = collection["relativeAtPreferredCountyStatus"];
                    cases.Student.Client.Client_StudyServiceDetails.First().HearAboutUs = collection["hearAboutUs"];
                    cases.Student.Client.Client_StudyServiceDetails.First().HearAboutUsOther = collection["hearAboutUsOther"];

                    stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                    stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == clientId).ToList();

                    if (collection["hearAboutUs"] == "")
                    {
                        ModelState.AddModelError("title", "Please tell us how did you hear about us");
                        return View(stmodel);
                    }

                    db.ObjectStateManager.ChangeObjectState(cases, EntityState.Modified);

                    db.SaveChanges();
                    LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited Service Details of the Student" + cases.Student.Client.GivenName + " " + cases.Student.Client.LastName ), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully Edited Study Service Details");
                    return View("Refresh");
                    //TempData[SUCCESS_EDIT] = true;
                    //return View(stmodel);
                   // return RedirectToAction("EditStudyServiceDetails", "StudentCentre", new { message = "Successfully Edited" });
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} First exception caught.", e);
                Debug.WriteLine(e.InnerException);
                ModelState.AddModelError("", e);
            }
            return View(stmodel);
        }

        #endregion

        #region Edit Work Experiance
        [Authorize(Roles = "Super,student,Counsellor")]
        public ActionResult EditWorkExperiance(int id, int workId)
        {
            //if (TempData[SUCCESS_EDIT] != null)
            //{
            //    ViewBag.successEdit = true;
            //}
            StudentCentreModel stmodel = new StudentCentreModel();
            //int studentId = Convert.ToInt32(CookieHelper.StudentId);
            int studentId = id;
            var client = db.Students.Single(c => c.Student_Id == studentId);
            var cases = db.Cases.ToList().Where(x => x.Student_Id == studentId);
            stmodel.caseTable = cases.ToList();

            stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            //stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Client_Id == client.Client_Id).ToList();
            stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Id == workId).ToList();
            ViewBag.clientId = client.Client_Id;
            return View(stmodel);
        }

        [HttpPost]
        public ActionResult EditWorkExperiance(StudentCentreModel stmodel, FormCollection collection, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    //int studentId = Convert.ToInt32(CookieHelper.StudentId);
                    int studentId = id;
                    Case cases = new Case();
                    cases = db.Cases.Single(x => x.Student_Id == studentId);

                    int clientId = Convert.ToInt32(collection["item.Client_Id"]);
                    int workId = Convert.ToInt32(collection["item.Id"]);
                    cases.Student.Client.Client_Work_Experience.First().From_Date = Convert.ToDateTime(collection["item.From_Date"]);
                    cases.Student.Client.Client_Work_Experience.First().To_Date = Convert.ToDateTime(collection["item.To_Date"]);
                    cases.Student.Client.Client_Work_Experience.First().Country = collection["Country"];
                    cases.Student.Client.Client_Work_Experience.First().Employer = collection["employer"];
                    cases.Student.Client.Client_Work_Experience.First().Position = collection["position"];
                    cases.Student.Client.Client_Work_Experience.First().Duties = collection["duties"];
                    cases.Student.Client.Client_Work_Experience.First().Client_Reference_Contact_No = collection["Client_Reference_Contact_No"];
                    cases.Student.Client.Client_Work_Experience.First().Client_Reference_Email = collection["Client_Reference_Email"];

                    stmodel.caseTable = db.Cases.ToList().Where(x => x.Case_Id == cases.Case_Id).ToList();
                    stmodel.clientChildrenDetail = db.Client_Children_Detail.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientQualificationTable = db.Client_Qualification.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSkillTestTable = db.Client_SkillTest.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientSpouse = db.Client_Spouse.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientStudyServiceDetails = db.Client_StudyServiceDetails.ToList().Where(x => x.Client_Id == clientId).ToList();
                    stmodel.clientWorkExperience = db.Client_Work_Experience.ToList().Where(x => x.Id == workId).ToList();


                    if (collection["item.From_Date"] == "")
                    {
                        ModelState.AddModelError("From_Date", "Please Enter the From Date");
                        return View(stmodel);
                    }
                    if (collection["item.To_Date"] == "")
                    {
                        ModelState.AddModelError("To_Date", "Please Enter the To Date");
                        return View(stmodel);
                    }
                    if (collection["Country"] == "")
                    {
                        ModelState.AddModelError("To_Date", "Please Enter the Country Name");
                        return View(stmodel);
                    }
                    if (collection["employer"] == "")
                    {
                        ModelState.AddModelError("To_Date", "Please Enter the Employer Name");
                        return View(stmodel);
                    }
                    if (collection["position"] == "")
                    {
                        ModelState.AddModelError("To_Date", "Please Enter your position");
                        return View(stmodel);
                    }
                    if (collection["duties"] == "")
                    {
                        ModelState.AddModelError("To_Date", "Please Enter your duties");
                        return View(stmodel);
                    }
                    db.ObjectStateManager.ChangeObjectState(cases, EntityState.Modified);

                    db.SaveChanges();
                    LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username+" Edited Work Experiance Details of the Student " + cases.Student.Client.GivenName + " " + cases.Student.Client.LastName ), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
                  
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully edited Work Experiance");
                    return View("Refresh");
                    //TempData[SUCCESS_EDIT] = true;
                    //return RedirectToAction("EditWorkExperiance", "StudentCentre", new { message = "Successfully Edited" });
                    // return View(stmodel);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} First exception caught.", e);
                Debug.WriteLine(e.InnerException);
                ModelState.AddModelError("", e);
            }
            return View(stmodel);
        }

        #endregion

        #endregion

        #region STUDENT APPOINTMENT PROFILE
        [Authorize(Roles = "Super,student,Counsellor")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult StudentProfile(int id)
        {
            ViewBag.message = Request.QueryString["message"];

            StudentCentreModel stmodel = new StudentCentreModel();
            var student = db.Students.ToList().Where(c => c.Client_Id == id);
            var client = db.Clients.ToList().Where(x => x.Client_Id == id);
            stmodel.studentTable = student.ToList();
            stmodel.clientTable = client.ToList();
            return View(stmodel);
        }

        #endregion

        #region Private Methods
        [Authorize(Roles = "Super,student,Counsellor")]
        private void BindTitle(string selectedValue)
        {

            var title = Enumclass.GetTitle();
            ViewData["TitleValue"] = new SelectList(title, "Value", "Text", selectedValue);
        }

        private void BindMaritalStatus(string selectedValue)
        {
            var title = Enumclass.GetMaritalStatus();
            ViewData["MaritalStatus"] = new SelectList(title, "Value", "Text", selectedValue);
        }

        private void BindGender(string selectedValue)
        {
            var title = Enumclass.GetGender();
            ViewData["Gender"] = new SelectList(title, "Value", "Text", selectedValue);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public static string SUCCESS_EDIT = "SuccessfulEdit";
    }
}