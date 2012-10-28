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

namespace StormWeb.Controllers
{
    public class ClientController : Controller
    {
        //
        // GET: /Client/
        private StormDBEntities db = new StormDBEntities();
        //
        // GET: /ClientAlternative/
          [Authorize(Roles = "Super")]
        public ViewResult Index()
        {
            var clients = db.Clients.Include("Address");
            return View(clients.ToList());
        }

        //
        // GET: /ClientAlternative/Details/5
          [Authorize(Roles = "Super")]
        public ViewResult Details(int id)
        {
            Client client = db.Clients.Single(c => c.Client_Id == id);
            return View(client);
        }

        //
        // GET: /ClientAlternative/Edit/5
          [Authorize(Roles = "Super")]
        public ActionResult Edit(int id)
        {
            Client client = db.Clients.Single(c => c.Client_Id == id);
            ViewBag.Address_Id = new SelectList(db.Addresses, "Address_Id", "Street_Name", client.Address_Id);
            return View(client);
        }

        //
        // POST: /ClientAlternative/Edit/5

        [Authorize(Roles="Super")]
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


        [Authorize (Roles="Super")]
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

        public ActionResult Register()
        {
            ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName");
            ViewBag.RepresentedCountry = new SelectList(CountryHelper.GetRepresentedCountries(), "CountryCode", "CountryName");
            ViewBag.NationalityList = new SelectList(NationalityHelper.GetCountries(), "Nationality", "Nationality");
            ViewBag.Services = new SelectList(ServiceHelper.GetItems(), "ServiceType", "ServiceType");
            ViewBag.Status = new SelectList(StatusTypeHelper.GetStatus(), "StatusType", "StatusType");
            ViewBag.CourseLevel = new SelectList(CourseLevelHelper.GetItems(), "CourseLevel", "CourseLevel");

            IEnumerable<Branch> branch = db.Branches.ToList();
            ViewBag.Branch = branch;
           
            return View(new ClientViewModel());
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
            model.ClientModel.Dob = DateTime.ParseExact(fc["ClientModel.Dob"], "dd/MM/yyyy",System.Globalization.CultureInfo.InvariantCulture);
            model.ClientModel.Nationality = fc["ClientModel.Nationality"];
            model.ClientModel.Email = fc["ClientModel.Email"];
            model.ClientModel.ContactNumber = fc["ClientModel.ContactNumber"];
            model.ClientModel.SecondaryContactNumber = fc["ClientModel.SecondaryContactNumber"];
            model.ClientModel.Address = new Address();
            model.ClientModel.Address.Address_Name = Convert.ToInt32(fc["AddressModel.Apartment_Number"]) + ", " + fc["AddressModel.Street_Name"];
            model.ClientModel.Address.State = fc["AddressModel.State"];
            model.ClientModel.Address.City = fc["AddressModel.City"];
            model.ClientModel.Address.Country_Id = Convert.ToInt32(fc["AddressModel.Country_Id"]);
            model.ClientModel.Address.Zipcode = Convert.ToInt32(fc["AddressModel.Zipcode"]);
            model.ClientModel.PreferredCountry = fc["ClientModel.PreferredCountry"];
            model.ClientModel.Services = fc["ClientModel.Services"];            
            model.ClientModel.Registered_On = DateTime.Now;
            model.ClientModel.Branch_Id = Convert.ToInt32(fc["selectedBranch"]);


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
                        Debug.Write(fc["Password"]);

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

                            // Create new Case entry in Database
                            Case nCase = new Case();
                            nCase.Branch_Id = (int)model.ClientModel.Branch_Id;
                            nCase.Status = CaseController.CASE_INITIATED;
                            nCase.Student_Id = student.Student_Id;
                            nCase.CreatedDate = DateTime.Now;
                            db.Cases.AddObject(nCase);
                            db.SaveChanges();

                            // Create entries for General documents to be uploaded by the students
                            foreach (CaseDoc_Template cdTemplates in db.CaseDoc_Template.Where(x => x.Required == true).ToList())
                            {
                                CaseDocument cd = new CaseDocument();
                                cd.CaseDocTemplate_Id = cdTemplates.CaseDocTemplate_Id;
                                cd.Case_Id = nCase.Case_Id;
                                db.CaseDocuments.AddObject(cd);                                
                            }
                            db.SaveChanges();


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

        // Request appointment for partially registered client
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
                    return RedirectToAction("AppointmentSuccess", "Client", new { id=model.Appointment.Appointment_Id });
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

        public ActionResult AppointmentSuccess(int id)
        {
            Appointment app = db.Appointments.SingleOrDefault(x => x.Appointment_Id == id);

            Branch b = db.Branches.SingleOrDefault(x => x.Branch_Id == app.Branch_Id);

            ViewBag.AppDateTime = app.AppDateTime;

            return View(b);
        }

        // GET: /Default1/Delete/5

        [Authorize(Roles="Super")]
        public ActionResult Delete(int id)
        {            
            Client client = db.Clients.First(i => i.Client_Id == id);

            // Delete addresses
            var addressToDelete = db.Addresses.Where(p => p.Address_Id.Equals(client.Client_Id));

            // Delete general enquiries
            var enquiriesToDelete = db.General_Enquiry.Where(p => p.Client_Id.Equals(client.Client_Id));

            /** TODO
             * 
             *  Delete from Student, Client_* tables
             * 
             * 
             * */

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
            
            // Delete 
            try
            {
                db.Clients.DeleteObject(client);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
                throw;
            }

            return RedirectToAction("Index");
        }

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
