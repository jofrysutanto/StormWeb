using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Helper;
using StormWeb.Models.ModelHelper;
using System.Diagnostics;
using System.Web.Security;
using System.Data;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Mail;

namespace StormWeb.Controllers
{
    public class ClientController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        // Checking email
        private bool invalid = false;

        [Authorize(Roles = "Super,BranchManager")]
        public ViewResult Index()
        {
            var clients = db.Clients.Include("Address");
            return View(clients.ToList());
        }

        [Authorize(Roles = "Super,BranchManager")]
        public ViewResult Details(int id)
        {
            Client client = db.Clients.SingleOrDefault(c => c.Client_Id == id);
            if (client != null)
            {
                return View(client);
            }
            else
            {
                return View(new Client());
            }
        }

        #region EDIT

        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult Edit(int id)
        {
            Client client = db.Clients.Single(c => c.Client_Id == id);
            // ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name", client.Address_Id);
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name", client.Address_Id);
            return View(client);
        }


        [Authorize(Roles = "Super,BranchManager")]
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
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Address_Name", client.Address_Id);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited the client Details " + client.GivenName + " " + client.LastName), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
            return View(client);
        }

        #endregion

        [Authorize(Roles = "Super,BranchManager")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult List()
        {
            string msg = Request.QueryString["message"];

            if (!string.IsNullOrEmpty(msg))
            {
                if (msg.Equals("register_success"))
                {
                    ViewBag.Msg = "<div class='success'>" + Request.QueryString["registered_person"] + " successfully registered</div><span class='clearFix'>&nbsp;</span>";
                }
            }

            ViewBag.Roles = CookieHelper.Roles;
            return View(db.Clients.ToList());
        }

        #region REGISTER

        public ActionResult ChooseRegistration()
        {
            return View();
        }

        public ActionResult Enquire()
        {
            FillRegistrationViewBagComponent();

            return View();
        }

        private void FillRegistrationViewBagComponent()
        {
            ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName");
            ViewBag.RepresentedCountry = new SelectList(CountryHelper.GetRepresentedCountries(), "CountryCode", "CountryName");
            ViewBag.NationalityList = new SelectList(NationalityHelper.GetCountries(), "Nationality", "Nationality");
            ViewBag.Services = new SelectList(ServiceHelper.GetItems(), "ServiceType", "ServiceType");
            ViewBag.Status = new SelectList(StatusTypeHelper.GetStatus(), "StatusType", "StatusType");
            ViewBag.CourseLevel = new SelectList(CourseLevelHelper.GetItems(), "CourseLevel", "CourseLevel");
            ViewBag.AssociateList = new SelectList(AssociateListHelper.GetItems(), "AssociateID", "AssociateName", new AssociateListHelper { AssociateID = 0, AssociateName = "--None--" });

            IEnumerable<Branch> branch = db.Branches.ToList();
            ViewBag.Branch = branch;
        }


        public JsonResult CheckPreviousRegistration(string id)
        {
            Client c;

            Dictionary<string, string> dict = new Dictionary<string, string>();

            bool smth1 = checkAllNumber(id);
            bool smth2 = IsValidEmail(id);

            // Fail number and email test
            if (!checkAllNumber(id) && !IsValidEmail(id))
            {
                dict.Add("result", "bad");
                return Json(dict);
            }

            if (checkAllNumber(id))
            {
                int testClientID = Convert.ToInt32(id);
                c = db.Clients.DefaultIfEmpty(null).SingleOrDefault(x => x.Client_Id == testClientID);
            }
            else
                c = null;

            // Not Client, check email
            if (c == null)
            {
                c = db.Clients.DefaultIfEmpty(null).SingleOrDefault(x => x.Email == id);

                if (c == null)
                {
                    dict.Add("result", "bad");
                    return Json(dict);
                }
            }


            // !! IMPORTANT !! 
            // Should check if this client ID already have username

            dict.Add("clientID", Convert.ToString(c.Client_Id));
            dict.Add("result", "good");

            return Json(dict);
        }

        private bool checkAllNumber(string str)
        {
            int num;
            bool isNum = int.TryParse(str, out num);
            if (isNum)
                return true;
            else
                return false;
        }

        private bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names. 
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper,
                                    RegexOptions.None);
            }
            catch (Exception)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format. 
            try
            {
                return Regex.IsMatch(strIn,
                    @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                    RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }


        public ActionResult Register(int id = -1)
        {
            FillRegistrationViewBagComponent();

            if (id <= -1)
            {
                ViewBag.RegisterType = "full";
                return View(new ClientViewModel());
            }
            else
            {
                ViewBag.RegisterType = "continue";
                Client c = db.Clients.SingleOrDefault(x => x.Client_Id == id);

                ClientViewModel cViewModel = new ClientViewModel();
                cViewModel.ClientModel = c;
                return View("RegisterContinue", cViewModel);
            }
        }

        [HttpPost]
        public ActionResult RegisterAccount(FormCollection fc)
        {
            // Attempt to register the user
            string username = fc["Username"];
            string password = fc["Password"];
            int clientID = Convert.ToInt32(fc["clientID"]);

            Client c = db.Clients.SingleOrDefault(x => x.Client_Id == clientID);

            // Check for existing user account for this client ID
            Student s = db.Students.SingleOrDefault(x => x.Client_Id == clientID);

            if (s != null)
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "You already have an account");
                return RedirectToAction("LogOn", "Account");
            }

            MembershipCreateStatus createStatus;
            Membership.CreateUser(username, password, c.Email, null, null, true, null, out createStatus);
            Roles.AddUserToRole(fc["Username"], "Student");

            if (createStatus == MembershipCreateStatus.Success)
            {
                //FormsAuthentication.SetAuthCookie(fc["Username"], false /* createPersistentCookie */);
                // Create new student entry in Database
                Student student = new Student();
                student.Client_Id = Convert.ToInt32(clientID);
                student.UserName = username;
                db.Students.AddObject(student);
                db.SaveChanges();

                Case nCase = AddNewCase(c.Branch_Id, student);
                AddDocumentTemplate(nCase);
                db.SaveChanges();

                return RedirectToAction("LogOn", "Account", new { message = "registration-success" });
            }
            else
            {
                // Some error happened
                ModelState.AddModelError("", ErrorCodeToString(createStatus));
            }

            NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Error handling your request!");
            return RedirectToAction("LogOn", "Account");
        }

        [HttpPost]
        public ActionResult Register(FormCollection fc)
        {
            ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName");
            ViewBag.RepresentedCountry = new SelectList(CountryHelper.GetRepresentedCountries(), "CountryCode", "CountryName");
            ViewBag.NationalityList = new SelectList(NationalityHelper.GetCountries(), "Nationality", "Nationality");
            ViewBag.Services = new SelectList(ServiceHelper.GetItems(), "ServiceType", "ServiceType");
            ViewBag.Status = new SelectList(StatusTypeHelper.GetStatus(), "StatusType", "StatusType");
            ViewBag.CourseLevel = new SelectList(CourseLevelHelper.GetItems(), "CourseLevel", "CourseLevel");

            IEnumerable<Branch> branch = db.Branches.ToList();
            ViewBag.Branch = branch;

            ClientViewModel model = new ClientViewModel();

            // Manually adding data into the model (sadly)
            model.ClientModel = new Client();
            model.ClientModel.Title = fc["ClientModel.Title"];
            model.ClientModel.GivenName = fc["ClientModel.GivenName"];
            model.ClientModel.LastName = fc["ClientModel.LastName"];
            model.ClientModel.Dob = DateTime.ParseExact(fc["ClientModel.Dob"], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            model.ClientModel.Nationality = fc["ClientModel.Nationality"];
            model.ClientModel.Email = fc["ClientModel.Email"];
            model.ClientModel.ContactNumber = fc["ClientModel.ContactNumber"];
            model.ClientModel.SecondaryContactNumber = fc["ClientModel.SecondaryContactNumber"];
            model.ClientModel.Address = new Address();
            model.ClientModel.Address.Address_Name = fc["AddressModel.Address_Name"];
            model.ClientModel.Address.State = fc["AddressModel.State"];
            model.ClientModel.Address.City = fc["AddressModel.City"];
            model.ClientModel.Address.Country_Id = Convert.ToInt32(fc["AddressModel.Country_Id"]);
            model.ClientModel.Address.Zipcode = Convert.ToInt32(fc["AddressModel.Zipcode"]);
            model.ClientModel.PreferredCountry = fc["ClientModel.PreferredCountry"];
            model.ClientModel.Services = fc["ClientModel.Services"];
            model.ClientModel.Registered_On = DateTime.Now;
            model.ClientModel.Branch_Id = Convert.ToInt32(fc["selectedBranch"]);

            // Associate ID (Optional)
            if (Convert.ToInt32(fc["ClientModel.Associate_Id"]) <= 0)
                model.ClientModel.Associate_Id = null;
            else
                model.ClientModel.Associate_Id = Convert.ToInt32(fc["ClientModel.Associate_Id"]);


            // Partial registration (user choose to book an appointment for enquiry
            if (!(fc["isFullRegistration"] == "yes"))
            {
                try
                {
                    //if (ModelState.IsValid)
                    //if (TryValidateModel(model.ClientModel))
                    //{
                    ValidateModel(model.ClientModel);
                    db.Clients.AddObject(model.ClientModel);
                    db.SaveChanges();
                    return RedirectToAction("RequestAppointment", "Client", new { id = model.ClientModel.Client_Id });
                    //}
                }
                catch (Exception e)
                {
                    Debug.WriteLine("{0} First exception caught.", e);
                    Debug.WriteLine(e.InnerException);
                    ModelState.AddModelError("", e);
                }
            }
            // If Full Registration
            else
            {
                try
                {
                    //if (ModelState.IsValid)
                    if (TryValidateModel(model.ClientModel))
                    {

                        // Attempt to register the user
                        string username = fc["Username"];
                        string password = fc["Password"];
                        MembershipCreateStatus createStatus;
                        Membership.CreateUser(username, password, model.ClientModel.Email, null, null, true, null, out createStatus);
                        Roles.AddUserToRole(fc["Username"], "Student");

                        if (createStatus == MembershipCreateStatus.Success)
                        {
                            //FormsAuthentication.SetAuthCookie(fc["Username"], false /* createPersistentCookie */);
                            // Create new student entry in Database
                            Student student = new Student();
                            student.Client_Id = model.ClientModel.Client_Id;
                            student.UserName = username;
                            model.ClientModel.Students.Add(student);

                            db.Clients.AddObject(model.ClientModel);
                            db.SaveChanges();

                            Case nCase = AddNewCase(model.ClientModel.Branch_Id, student);

                            AddDocumentTemplate(nCase);
                            db.SaveChanges();

                            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Registered a new client " + model.ClientModel.GivenName + " " + model.ClientModel.LastName), LogHelper.LOG_OTHER, LogHelper.SECTION_CLIENT);
                            return RedirectToAction("LogOn", "Account", new { message = "registration-success" });
                        }
                        else
                        {
                            ModelState.AddModelError("", ErrorCodeToString(createStatus));
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("{0} First exception caught.", e);
                    Debug.WriteLine(e.InnerException);
                    ModelState.AddModelError("", e);
                }
            }
            return View(model);
        }

        private Case AddNewCase(int branchID, Student student)
        {
            // Create new Case entry in Database
            Case nCase = new Case();
            nCase.Branch_Id = branchID;
            nCase.Status = CaseController.CASE_INITIATED;
            nCase.Student_Id = student.Student_Id;
            nCase.CreatedDate = DateTime.Now;
            db.Cases.AddObject(nCase);
            db.SaveChanges();
            return nCase;
        }

        private void AddDocumentTemplate(Case nCase)
        {
            // Create entries for General documents to be uploaded by the students
            foreach (CaseDoc_Template cdTemplates in db.CaseDoc_Template.Where(x => x.Required == true).ToList())
            {
                CaseDocument cd = new CaseDocument();
                cd.CaseDocTemplate_Id = cdTemplates.CaseDocTemplate_Id;
                cd.Case_Id = nCase.Case_Id;
                db.CaseDocuments.AddObject(cd);
            }
        }

        #endregion

        #region Request appointment for partially registered client

        public ActionResult RequestAppointment(int id)
        {
            // Check if there is already appointment for this client id
            General_Enquiry ge = db.General_Enquiry.DefaultIfEmpty(null).SingleOrDefault(x => x.Client_Id == id);

            if (ge != null)
                return RedirectToAction("LogOn", "Account");

            General_Enquiry model = new General_Enquiry();
            model.Client_Id = id;

            IEnumerable<Branch> branch = db.Branches.ToList();
            ViewBag.Branch = branch;


            Client c = db.Clients.Single(x => x.Client_Id == id);
            model.Branch_Id = c.Branch_Id;
            ViewBag.ClientName = c.GivenName + " " + c.LastName;

            return View(model);
        }

        // Request appointment for partially registered client
        [HttpPost]
        public ActionResult RequestAppointment(General_Enquiry model, FormCollection fc)
        {
            // Check if there is already appointment for this client id
            General_Enquiry ge = db.General_Enquiry.DefaultIfEmpty(null).SingleOrDefault(x => x.Client_Id == model.Client_Id);

            if (ge != null)
                return RedirectToAction("LogOn", "Account");

            ViewBag.Branch = db.Branches.ToList();

            model.Appointment.AppDateTime = Utilities.formatDate(fc["Appointment.AppDateTime"], "dd/MM/yyyy");
            ModelState.Remove("Appointment.AppDateTime");

            try
            {
                if (ModelState.IsValid)
                {
                    model.Appointment.Branch_Id = model.Branch_Id;
                    model.Appointment.Confirmation = AppointmentController.APP_REQUEST_APPROVAL;
                    db.General_Enquiry.AddObject(model);
                    db.SaveChanges();
                    LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + "Requested appointment "), LogHelper.LOG_OTHER, LogHelper.SECTION_CLIENT);

                    string messageBody = "<div>Your appointment is scheduled on: " + model.Appointment.AppDateTime.ToShortDateString() + " on " + model.Appointment.AppDateTime.ToShortTimeString() + "</div>";

                    messageBody += "<div><br/> <strong>This is your client ID for future reference: " + model.Client_Id + "</strong></div>";

                    EmailHelper.sendEmail(new MailAddress(model.Client.Email), "We have received your appointment request", messageBody);
                    return RedirectToAction("AppointmentSuccess", "Client", new { id = model.Appointment.Appointment_Id });
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} First exception caught.", e);
                Debug.WriteLine(e.InnerException);
                ModelState.AddModelError("", e);
            }

            Client c = db.Clients.Single(x => x.Client_Id == model.Client_Id);
            ViewBag.ClientName = c.GivenName + " " + c.LastName;

            return View(model);
        }

        #endregion

        public ActionResult AppointmentSuccess(int id)
        {
            Appointment app = db.Appointments.SingleOrDefault(x => x.Appointment_Id == id);

            Branch b = db.Branches.SingleOrDefault(x => x.Branch_Id == app.Branch_Id);

            General_Enquiry g = db.General_Enquiry.SingleOrDefault(x => x.Appointment_Id == id);

            ViewBag.clientId = g.Client_Id;

            ViewBag.clientName = g.Client.GivenName;

            ViewBag.AppDateTime = app.AppDateTime;

            return View(b);
        }

        #region DELETE

        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult Delete(int id)
        {
            Client client = db.Clients.First(i => i.Client_Id == id);
            return View(client);
        }

        [HttpPost]
        [Authorize(Roles = "Super,BranchManager")]
        public ActionResult Delete(int id, Client client1)
        {
            Client client = db.Clients.First(i => i.Client_Id == id);

            // Delete addresses
            var addressToDelete = db.Addresses.Where(p => p.Address_Id.Equals(client.Client_Id));

            // Delete general enquiries
            var enquiriesToDelete = db.General_Enquiry.Where(p => p.Client_Id.Equals(client.Client_Id));

            //Delete student 
            var studentsToDelete = db.Students.Where(p => p.Client_Id.Equals(client.Client_Id));

            //Delete Cases
            var caseToDelete = db.Cases.Where(p => p.Student_Id.Equals(studentsToDelete.FirstOrDefault().Student_Id));

            //Delete Cases_Staff
            var caseStaffToDelete = db.Case_Staff.Where(p => p.Case_Id.Equals(caseToDelete.FirstOrDefault().Case_Id));

            //Delete CasesDocument
            var caseDocToDelete = db.CaseDocuments.Where(p => p.Case_Id.Equals(caseToDelete.FirstOrDefault().Case_Id));
            
          

            //Delete All Applications
            var applicationsToDelete = db.Applications.Where(p => p.Case_Id.Equals(caseToDelete.FirstOrDefault().Case_Id));
            if (applicationsToDelete.Count() > 0)
                ApplicationController.deleteAllApplications(applicationsToDelete.FirstOrDefault().Application_Id);

            //Delete All Log
            var logToDelete = db.Student_Log.Where(p => p.UserName.Equals(studentsToDelete.FirstOrDefault().UserName));

            if (addressToDelete.Count() > 0)
            {
                foreach (Address address in addressToDelete)
                {
                    db.Addresses.DeleteObject(address);
                }
            }

            if (enquiriesToDelete.Count() > 0)
            {
                foreach (General_Enquiry generalEnquiry in enquiriesToDelete)
                {
                    db.General_Enquiry.DeleteObject(generalEnquiry);
                }
            }

            if (studentsToDelete.Count() > 0)
            {
                foreach (Student student in studentsToDelete)
                {
                    db.Students.DeleteObject(student);
                }
            }

            if (caseToDelete.Count() > 0)
            {
                foreach (Case cases in caseToDelete)
                {
                    db.Cases.DeleteObject(cases);
                }
            }

            if (caseStaffToDelete.Count() > 0)
            {
                foreach (Case_Staff cases in caseStaffToDelete)
                {
                    db.Case_Staff.DeleteObject(cases);
                }
            }
            if (caseDocToDelete.Count() > 0)
            {
                foreach (CaseDocument cases in caseDocToDelete)
                {
                    db.CaseDocuments.DeleteObject(cases);
                }
            } 
                

            if (logToDelete.Count() > 0)
            {
                foreach (Student_Log studentLog in logToDelete)
                {
                    db.Student_Log.DeleteObject(studentLog);
                }
            } 
            // Delete 
            try
            {
                db.Clients.DeleteObject(client);
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Deleted the client Details of" + client.GivenName + " " + client.LastName), LogHelper.LOG_DELETE, LogHelper.SECTION_CLIENT);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
                throw;
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Email Check

        public JsonResult IsEmailExist([Bind(Prefix = "ClientModel.Email")]string email)
        {
            JsonResult result = new JsonResult();

            Client c = db.Clients.DefaultIfEmpty(null).SingleOrDefault(x => x.Email == email);

            if (c == null)
            {
                result.Data = true;
            }
            else
                result.Data = false;

            return result;
        }

        #endregion

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
