// --------------------------------------------------------------------------------------------------------------------
// <summary>
// File Name    : DocumentController.cs
// Created Date : 15/08/2012
// Created By   : Maysa L. Hakmeh
// Description  : A controller to manage student's application documents 
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Controllers;
using System.IO;
using StormWeb.Helper;
using System.Data.Objects.SqlClient;
using System.Web.Script.Serialization;
using Amazon.S3.Model;
using Amazon.S3;
using System.Collections.Specialized;
using System.Configuration;
using Amazon;
using System.Net;
using System.Text;

namespace StormWeb.Controllers
{
    public class DocumentController : Controller
    {
        private string accessKey = "";
        private string secretKey = "";
        private string bucketName = "";

        private StormDBEntities db = new StormDBEntities();
        private string fileName;
        private string path;
        int stid = 0;
        int staff = 0;


        #region Councelor  page

        // GET: /CaseTemp/
        // This will show all  Case Document Templates from student page to select from them
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public ViewResult ShowGeneralTemplates()
        {
            return View(db.CaseDoc_Template.ToList());
            //return View();
        }




        // This will show all  Case Document Templates from student page to select from them
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public ViewResult ShowAllCaseTemplates(int id = 0)
        {
            if (id != 0)
            {
                ViewBag.clientId = id;
                int stid = db.Students.Single(x => x.Client_Id == id).Student_Id;
                int caseId = db.Cases.Single(x => x.Student_Id == stid).Case_Id;
                // create a list of all not uploaded documents
                ViewBag.listID = db.CaseDocuments.Where(x => x.Case_Id == caseId && x.UploadedOn == null).Select(x => x.CaseDocTemplate_Id).ToList();
                ViewBag.listIdDisabled = db.CaseDocuments.Where(x => x.Case_Id == caseId && x.UploadedOn != null).Select(x => x.CaseDocTemplate_Id).ToList();

                return View(db.CaseDoc_Template.ToList());
            }


            return View(db.CaseDoc_Template.ToList());
            //return View();
        }


        public bool checkRequiredFields()
        {
            ServiceConfiguration config = new ServiceConfiguration();

            accessKey = config.AWSAccessKey;
            secretKey = config.AWSSecretKey;
            bucketName = config.BucketName;

            if (string.IsNullOrEmpty(accessKey))
            {
                Console.WriteLine("AWSAccessKey was not set in the App.config file.");
                return false;
            }
            if (string.IsNullOrEmpty(secretKey))
            {
                Console.WriteLine("AWSSecretKey was not set in the App.config file.");
                return false;
            }
            if (string.IsNullOrEmpty(bucketName))
            {
                Console.WriteLine("The variable bucketName is not set.");
                return false;
            }

            return true;
        }


        [Authorize(Roles = "Super,Counsellor,Administrator")]
        [HttpPost]
        public ActionResult ShowAllCaseTemplates(int id, FormCollection formCollection)
        {
            int stid = db.Students.Single(x => x.Client_Id == id).Student_Id;
            int caseId = db.Cases.Single(x => x.Student_Id == stid).Case_Id;


            var oldCaseDoc = db.CaseDocuments.Where(x => x.Case_Id == caseId).ToList();
            // create a list of all  case  old general documents ID's 
            List<int?> oldlistID = db.CaseDocuments.Where(x => x.Case_Id == caseId).Select(x => x.CaseDocTemplate_Id).ToList();
            // create a list of all  case  new general documents ID's 
            List<int> newListID = new List<int>();

            var fc = formCollection["listID"].Split(',');
            foreach (var input in fc)
            {

                newListID.Add(Int32.Parse(input));
            }


            foreach (int? item in oldlistID)
            {
                if (item != null)
                {
                    //newListID.Remove((int)item);
                    CaseDocument casedoc = db.CaseDocuments.DefaultIfEmpty(null).SingleOrDefault(c => c.CaseDocTemplate_Id == item && c.Case_Id == caseId && c.UploadedOn == null);

                    if (casedoc != null)
                    {
                        db.CaseDocuments.DeleteObject(casedoc);
                        db.SaveChanges();
                        LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (casedoc.CaseDocTemplate_Id + " Case Template is Deleted "), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

                    }
                }
            }

            if (ModelState.IsValid)
            {

                foreach (int item in newListID)
                {
                    // Add the corresponding Template into the Case Docuemnt


                    CaseDocument found = (from e in db.CaseDocuments
                                          where e.CaseDocTemplate_Id == item && e.Case_Id == caseId
                                          select e).DefaultIfEmpty(null).SingleOrDefault();

                    // add a new record only if doesnt exist in the file                                       
                    if (found == null)
                    {
                        CaseDocument doc = new CaseDocument();

                        doc.CaseDocTemplate_Id = item;
                        doc.Case_Id = caseId;
                        db.CaseDocuments.AddObject(doc);
                    }

                }
                db.SaveChanges();

            }
            return RedirectToAction("StudentDetails", new { id = id });

        }

        // GET: /CaseTemp/Create

        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public ActionResult CreateCaseTemp(int returnId = 0)
        {
            ViewBag.clientId = returnId;
            return View();
        }

        //
        // POST: /CaseTemp/Create

        [HttpPost]
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public ActionResult CreateCaseTemp(CaseDoc_Template casedoc_template, HttpPostedFileBase file, int returnId = 0)
        {


            if (casedoc_template.Downloadable == true && file.FileName == null)
            {
                ModelState.AddModelError("FileNameError", "Please select file!");
                return View();
            }

            if (casedoc_template.Downloadable == true && file.FileName != null)
            {
                string pathToCreate;
                pathToCreate = TEMPLATE_GENERAL_PATH;
                string fileToCreate = pathToCreate + '/' + file.FileName;
                casedoc_template.FileName = file.FileName;
                casedoc_template.Path = pathToCreate;


                uploadAWS(fileToCreate, file);
            }
            // create / update folder 
            casedoc_template.UploadedOn = System.DateTime.Now;
            casedoc_template.UploadedBy = CookieHelper.Username;
            db.CaseDoc_Template.AddObject(casedoc_template);
            if (ModelState.IsValid)
            {

                db.SaveChanges();

                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (casedoc_template.CaseDocTemplate_Id + " Case Template is Created "), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Template Was Created Successfully!");
                return View("Refresh");

            }
            else
            {
                ModelState.AddModelError("NameError", "Please enter name !");
                return View();
            }

        }
        //if (returnId != 0)
        //{
        //    return RedirectToAction("ShowAllCaseTemplates", new { id = returnId });
        //}
        //else
        //{
        //    return RedirectToAction("ShowGeneralTemplates");
        //}
        //return View(casedoc_template);


        // GET: /CaseTemp/Edit/5
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public ActionResult EditCaseTemp(int id, int returnId = 0)
        {
            ViewBag.clientId = returnId;
            CaseDoc_Template casedoc_template = db.CaseDoc_Template.Single(c => c.CaseDocTemplate_Id == id);
            return View(casedoc_template);
        }

        //
        // POST: /CaseTemp/Edit/5

        [HttpPost]
        public ActionResult EditCaseTemp(CaseDoc_Template casedoc_template, int returnId = 0)
        {
            if (ModelState.IsValid)
            {
                db.CaseDoc_Template.Attach(casedoc_template);
                db.ObjectStateManager.ChangeObjectState(casedoc_template, EntityState.Modified);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (casedoc_template.CaseDocTemplate_Id + " Case Template is edited "), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

            }
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Template Was Modified Successfully!");
            return View("Refresh");
            //if (returnId != 0)
            //{
            //    //return RedirectToAction("ShowAllCaseTemplates", new { id = returnId });
            //    return View("Refresh");
            //}
            //else
            //{
            // return RedirectToAction("ShowGeneralTemplates");

            //}
        }


        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public ActionResult DeleteCaseTemp(int id, int returnId = 0)
        {
            CaseDocument found = (from e in db.CaseDocuments
                                  where e.CaseDocTemplate_Id == id
                                  select e).DefaultIfEmpty(null).FirstOrDefault();

            CaseDoc_Template casedoc_template = db.CaseDoc_Template.DefaultIfEmpty(null).SingleOrDefault(c => c.CaseDocTemplate_Id == id);

            if ((casedoc_template != null && casedoc_template.Required == false) && (found == null))
            {

                db.CaseDoc_Template.DeleteObject(casedoc_template);

                if (casedoc_template.FileName != null)
                {
                    //System.IO.File.Delete(completFileName);
                    DeletingAWS(casedoc_template.Path + '/' + casedoc_template.FileName);
                }
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (casedoc_template.CaseDocTemplate_Id + " Case Template is Deleted "), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Template was deleted successfully!");

            }

            else
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Template cannot be deleted as it is either used in another application or it is required!");

            }


            if (returnId != 0)
            {
                return RedirectToAction("ShowAllCaseTemplates", new { id = returnId });
            }
            else
            {
                return RedirectToAction("ShowGeneralTemplates");
            }
        }




        /////  StudentDetails    //////////////////////////////
        // 
        /// The following methods are used to display details of selected student 
        /// ////////////////////////////////////////////////////////////////////////
        [Authorize]
        public ViewResult StudentDetailsCompleted(int id)
        {
            int stid = db.Students.Single(x => x.Client_Id == id).Student_Id;
            int caseId = db.Cases.Single(x => x.Student_Id == stid).Case_Id;
            string name = db.Clients.Single(x => x.Client_Id == id).GivenName + " " + db.Clients.Single(x => x.Client_Id == id).LastName;
            var applications = db.Applications.ToList().Where(c => c.Student_Id == stid && c.Completed == true);
            ViewBag.clientId = id;
            ViewBag.casedocs = GetCaseItems(caseId);
            ViewBag.studentName = name;
            ViewBag.caseId = caseId;

            List<DocumentIndexViewModel> documentsViewModel = new List<DocumentIndexViewModel>();

            foreach (var item in applications)
            {
                List<Template_Document> appDocTemplates = db.Template_Document.Where(c => c.Course_Id == item.Course_Id).ToList();
                List<Application_Document> app = (from a in db.Applications
                                                  from t in db.Application_Document
                                                  where a.Student_Id == stid && a.Course_Id == item.Course_Id && a.Case_Id == caseId && t.Application_Id == a.Application_Id
                                                  select t).ToList();
                DocumentIndexViewModel docVM = new DocumentIndexViewModel(item.Course_Id);

                docVM.appDoc = app;
                docVM.tempDoc = appDocTemplates;
                docVM.courseId = item.Course_Id;

                documentsViewModel.Add(docVM);
            }

            ViewBag.DocumentViewModel = documentsViewModel;

            ViewBag.casedocs = GetCaseItems(caseId);

            //ViewBag.casedocs = casedocs;
            return View(applications);


        }

        [Authorize]
        public ViewResult StudentDetails(int id)
        {
            var students = db.Students.Single(x => x.Client_Id == id);
            int stid = 0;
            if (students != null)
                stid = students.Student_Id;
            int caseId = db.Cases.Single(x => x.Student_Id == stid).Case_Id;
            string name = db.Clients.Single(x => x.Client_Id == id).GivenName + " " + db.Clients.Single(x => x.Client_Id == id).LastName;

            // Getting all the applications with completed = false
            //var applications = db.Applications.ToList().Where(c => c.Student_Id == stid && (c.Completed == false || c.Completed == null));

            // Getting all the applications
            var applications = db.Applications.ToList().Where(c => c.Student_Id == stid);

            ViewBag.clientId = id;
            ViewBag.casedocs = GetCaseItems(caseId);
            ViewBag.studentName = name;
            ViewBag.caseId = caseId;
            BindCurrency("-Select Currency-");

            List<DocumentIndexViewModel> documentsViewModel = new List<DocumentIndexViewModel>();

            foreach (var item in applications)
            {
                List<Template_Document> appDocTemplates = db.Template_Document.Where(c => c.Course_Id == item.Course_Id).ToList();
                List<Application_Document> app = (from a in db.Applications
                                                  from t in db.Application_Document
                                                  where a.Student_Id == stid && a.Course_Id == item.Course_Id && a.Case_Id == caseId && t.Application_Id == a.Application_Id
                                                  select t).ToList();
                DocumentIndexViewModel docVM = new DocumentIndexViewModel(item.Course_Id);

                docVM.appDoc = app;
                docVM.tempDoc = appDocTemplates;
                docVM.courseId = item.Course_Id;

                documentsViewModel.Add(docVM);
            }

            ViewBag.DocumentViewModel = documentsViewModel;

            ViewBag.casedocs = GetCaseItems(caseId);

            //ViewBag.casedocs = casedocs;
            return View(applications);


        }
        // return all case documents for a selected student
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public List<CaseDocument> GetCaseItems(int id)
        {
            return db.CaseDocuments.Where(x => x.Case_Id == id).ToList();
        }


        // The following method is used to display the list of students of the login staff
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public ViewResult ShowAllStudents()
        {
            if (CookieHelper.isStaff())
            {
                staff = Convert.ToInt32(CookieHelper.StaffId);
            }
            else if (CookieHelper.isStudent())
            {
                stid = Convert.ToInt32(CookieHelper.StudentId);
            }


            List<Client> students = (from s in db.Students
                                     from cl in db.Clients
                                     from f in db.Case_Staff
                                     from c in db.Cases
                                     where f.Staff_Id == staff
                                     && c.Student_Id == s.Student_Id
                                     && s.Client_Id == cl.Client_Id
                                     && c.Case_Id == f.Case_Id
                                     select cl).ToList();
            return View(students);
        }


        [Authorize]
        public ActionResult ApproveDocument(int id, int returnId)
        {
            Application_Document application_document = db.Application_Document.Single(t => t.ApplicationDoc_Id == id);
            Application app = db.Applications.Single(a => a.Application_Id == application_document.Application_Id);
            string appStatus = db.Applications.Single(a => a.Application_Id == application_document.Application_Id).Status;


            // if application status = submitted , councelor cant unapprove the document
            int progressValue = (int)Enum.Parse(typeof(StormWeb.Controllers.ApplicationController.ApplicationStatusType), appStatus);
            if (progressValue < 60)
            {
                if (application_document.Approved == false || application_document.Approved == null)
                {
                    application_document.Approved = true;
                }
                else
                {
                    application_document.Approved = false;
                }
                // check all application documents for status update

                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (app.Application_Id + " Application has been Approved "), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);
                MessageController.sendSystemMessage(app.Student.UserName, "Document approval", "Your document " + application_document.Template_Document.Form_Name + " have been approved.");
            }
            else
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Application has been submitted !");
            }

            // check documents approval and Update Application status accordingly
            if (progressValue < 60)
            {
                // if all application documents are approved then application status will be "Documents Completed"
                if (StormWeb.Models.ModelHelper.DocumentHelper.getUnApprovedDocs(app.Application_Id) == 0)
                {
                    app.Status = ApplicationController.ApplicationStatusType.Documents_Completed.ToString();
                    //ApplicationController.statusUp(app.Application_Id);
                    MessageController.sendSystemMessage(app.Student.UserName, "All documents have been approved", "All your documents are approved, your application are now  waiting for submission.");
                }
                // else return application status to "staff assigned"
                else
                {
                    app.Status = ApplicationController.ApplicationStatusType.Staff_Assigned.ToString();
                    //ApplicationController.statusDown(app.Application_Id);
                }

            }

            db.SaveChanges();
            return RedirectToAction("StudentDetails", new { id = returnId });
        }


        #endregion

        #region Template Documents
        // document templates 
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ViewResult ShowAllDocumentTemplates()
        {

            var docs = db.Courses.ToList();
            return View(docs);
        }


        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public ActionResult EditDocTemp(int id)
        {
            Template_Document template_document = db.Template_Document.Single(t => t.TemplateDoc_Id == id);
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name", template_document.Course_Id);
            return View(template_document);
        }

        //
        // POST: /Template/Edit/5

        [HttpPost]
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public ActionResult EditDocTemp(Template_Document template_document)
        {
            if (ModelState.IsValid)
            {
                db.Template_Document.Attach(template_document);
                db.ObjectStateManager.ChangeObjectState(template_document, EntityState.Modified);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (template_document.TemplateDoc_Id + " Template Document is Edited  "), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

            }
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name", template_document.Course_Id);
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Template Was Modified Successfully!");
            return RedirectToAction("ShowAllDocumentTemplates");
        }


        #endregion

        #region List Of  Applications' Documents- INDEX

        // The following method is used to display the list of documents for a specific user
        [AcceptVerbs(HttpVerbs.Get)]
        [Authorize(Roles = "Student")]
        public ViewResult Index(int go = -1, bool faq = false)
        {

            BindCurrency("--Currency--");
            PopulatePayment(-1);
            // Determine which application to show on page load
            if (go >= 0)
                ViewBag.Go = go;

            // Show FAQ
            if (faq)
            {
                ViewBag.FAQ = true;
            }

            List<ApplicationDocumentViewModel> appViewModel = new List<ApplicationDocumentViewModel>();
            if ((string)TempData[SUCCESS_EDIT] == "true")
            {
                ViewBag.SuccessEdit = true;
            }

            if (CookieHelper.isStudent())
            {
                stid = Convert.ToInt32(CookieHelper.StudentId);
                //ViewBag.EditMessage = Request.QueryString["message"];

            }
            else
            {
                stid = -1;
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Invalid student!");
                RedirectToAction("Index", "Home");
            }

            var applications = db.Applications.ToList().Where(c => c.Student_Id == stid);


            List<CaseDocument> completedCaseDocs = new List<CaseDocument>();
            List<CaseDocument> pendingCaseDocs = new List<CaseDocument>();

            // Retrieve all Case Documents
            int caseId = db.Cases.Single(x => x.Student_Id == stid).Case_Id;
            var caseDocs = db.CaseDocuments.Where(x => x.Case_Id == caseId);
            foreach (CaseDocument cd in caseDocs)
            {
                if (cd.FileName == null)
                {
                    pendingCaseDocs.Add(cd);
                }
                else
                {
                    completedCaseDocs.Add(cd);
                }
            }

            // Retrieve all documents for this case
            foreach (var item in applications)
            {
                List<Application_Document> appDocs = item.Application_Document.ToList();

                ApplicationDocumentViewModel tempAppViewModel = new ApplicationDocumentViewModel();

                tempAppViewModel.app = item;


                foreach (Template_Document template in item.Course.Template_Document.ToList())
                {
                    Application_Document app = appDocs.SingleOrDefault(x => x.TemplateDoc_Id == template.TemplateDoc_Id);
                    // If file already uploaded for this template
                    if (appDocs.Where(x => x.TemplateDoc_Id == template.TemplateDoc_Id).Count() <= 0)
                    {
                        tempAppViewModel.notCompleted.Add(template);
                    }
                    else
                    {
                        tempAppViewModel.completed.Add(app);
                    }
                }

                tempAppViewModel.offer = item.Application_Result.SingleOrDefault(x => x.Type == OFFER_LETTER_TYPE);
                tempAppViewModel.coe = item.Application_Result.SingleOrDefault(x => x.Type == COE_TYPE);
                tempAppViewModel.acceptance = item.Application_Result.SingleOrDefault(x => x.Type == ACCEPTANCE_TYPE);
                tempAppViewModel.completedAcceptance = item.Application_Result.SingleOrDefault(x => x.Type == COMPLETED_ACCEPTANCE_TYPE);

                tempAppViewModel.notCompletedGeneralDocs = pendingCaseDocs;
                tempAppViewModel.completedGeneralDocs = completedCaseDocs;

                appViewModel.Add(tempAppViewModel);
            }

            List<DocumentIndexViewModel> documentsViewModel = new List<DocumentIndexViewModel>();

            foreach (var item in applications)
            {
                List<Template_Document> appDocTemplates = db.Template_Document.Where(c => c.Course_Id == item.Course_Id).ToList();
                List<Application_Document> app = (from a in db.Applications
                                                  from t in db.Application_Document
                                                  where a.Student_Id == stid && a.Course_Id == item.Course_Id && a.Case_Id == caseId && t.Application_Id == a.Application_Id
                                                  select t).ToList();
                DocumentIndexViewModel docVM = new DocumentIndexViewModel(item.Course_Id);

                docVM.appDoc = app;
                docVM.tempDoc = appDocTemplates;
                docVM.courseId = item.Course_Id;

                documentsViewModel.Add(docVM);

            }

            ViewBag.DocumentViewModel = documentsViewModel;

            ViewBag.casedocs = GetCaseItems(caseId);

            //ViewBag.casedocs = casedocs;
            return View(appViewModel);

        }

        public static DocumentIndexViewModel getDocumentViewModelByCourseId(List<DocumentIndexViewModel> list, int courseId)
        {
            foreach (DocumentIndexViewModel model in list)
            {
                if (model.courseId == courseId)
                    return model;
            }

            return null;
        }

        #endregion

        #region Deletes
        [Authorize]
        public ActionResult DeleteOfferLetter(int id, string doctype, int returnId, bool statusChange = true)
        {
            if (ModelState.IsValid)
            {
                Application_Result application = db.Application_Result.Single(a => a.Id == id);
                Application app = db.Applications.Single(a => a.Application_Id == application.Application_Id);

                string completFileName = Server.MapPath(application.Path + '/' + application.FileName);
                //System.IO.File.Delete(completFileName);
                DeletingAWS(application.Path + '/' + application.FileName);
                db.Application_Result.DeleteObject(application);

                if (statusChange)
                    ApplicationController.statusDown(application.Application_Id);

                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Delete Application Result  " + "-" + application.Id), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

            }

            return RedirectToAction("StudentDetails", new { id = returnId });
        }

        // GET: /DocTemp/Delete/5
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult DeleteDocTemp(int id)
        {
            Template_Document template_document = db.Template_Document.Single(t => t.TemplateDoc_Id == id);

            if (getAppByTemplate(id) == null)
            {
                //string completFileName = Server.MapPath(template_document.Path + '/' + template_document.FileName);
                //System.IO.File.Delete(completFileName);
                DeletingAWS(template_document.Path + '/' + template_document.FileName);

                db.Template_Document.DeleteObject(template_document);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (template_document.TemplateDoc_Id + " Template Document is Delel  "), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Template Was Deleted Successfully!");

            }


            else
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Template Cannot be Deleted as it is used in other applications!");

            }

            return RedirectToAction("ShowAllDocumentTemplates");
        }

        public static Application_Document getAppByTemplate(int id)
        {
            StormDBEntities db = new StormDBEntities();
            foreach (Application_Document ad in db.Application_Document)
            {
                if (ad.TemplateDoc_Id == id)
                {
                    return ad;
                }
            }

            return null;
        }


        [Authorize]
        public ActionResult DeleteAppDoc(int id)
        {
            if (ModelState.IsValid)
            {
                Application_Document application = db.Application_Document.Single(a => a.ApplicationDoc_Id == id);
                string completFileName = Server.MapPath(application.Path + '/' + application.FileName);
                //System.IO.File.Delete(completFileName);

                DeletingAWS(application.Path + '/' + application.FileName);
                db.Application_Document.DeleteObject(application);

                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (application.ApplicationDoc_Id + " Application Document has been Deleted  "), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

            }

            return RedirectToAction("Index");
        }


        [Authorize]
        public ActionResult DeleteCaseDoc(int id)
        {

            if (ModelState.IsValid)
            {
                CaseDocument application = db.CaseDocuments.Single(a => a.CaseDocument_Id == id);
                // delete file from student's folder
                string completFileName = Server.MapPath(application.Path + '/' + application.FileName);
                //System.IO.File.Delete(completFileName);

                DeletingAWS(application.Path + '/' + application.FileName);
                // modify database
                application.Path = null;
                application.UploadedOn = null;
                application.FileName = null;
                application.Comment = null;

                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (application.CaseDocument_Id + " Case Document is Deleted "), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

            }

            return RedirectToAction("Index");
        }

        #endregion

        #region AWS
        public ViewResult viewAWS()
        {
            string result = "";
            if (!checkRequiredFields())
            {
                ViewBag.Result = "";
                return View();
            }
            AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.APSoutheast1);

            ListObjectsRequest request = new ListObjectsRequest();


            request.BucketName = bucketName;
            using (ListObjectsResponse response = client.ListObjects(request))
            {
                foreach (S3Object entry in response.S3Objects)
                {
                    result += "key = " + entry.Key + " size = " + entry.Size + "<br/>";
                }
            }
            ViewBag.Result = result;
            return View();
        }


        //ViewBag.Result = result;
        //return View();


        //string result = "";

        //try
        //{

        //    ListObjectsRequest request = new ListObjectsRequest();
        //    request.BucketName = bucketName;

        //    PutObjectRequest uploadrequest = new PutObjectRequest();
        //    uploadrequest.WithContentBody("this is a test")
        //        .WithBucketName(bucketName)
        //        .WithKey("testupload.pdf")
        //        .WithContentType("applicaton/pdf")

        //        //.WithFilePath("Upload/2_Stewie")
        //        .WithInputStream(file.InputStream);


        //    S3Response uploadResponse = client.PutObject(uploadrequest);
        //    uploadResponse.Dispose();
        //}
        //catch (AmazonS3Exception amazonS3Exception)
        //{
        //    if (amazonS3Exception.ErrorCode != null && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
        //    {
        //        Console.WriteLine("Please check the provided AWS Credentials.");
        //        Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
        //    }
        //    else
        //    {
        //        Console.WriteLine("An error occurred with the message '{0}' when listing objects", amazonS3Exception.Message);
        //    }
        //}

        //using (ListObjectsResponse response = client.ListObjects(request))
        //{
        //    foreach (S3Object entry in response.S3Objects)
        //    {
        //        result += "key = " + entry.Key + " size = " + entry.Size + "<br/>";
        //    }
        //}

        // simple object put


        //ViewBag.Result = result;
        //return View();



        // upload to Amazon  s3
        [HttpPost]
        public ActionResult uploadAWS(string path, HttpPostedFileBase file)
        {

            if (!checkRequiredFields())
            {
                ViewBag.Result = "";
                return View();
            }
            AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.APSoutheast1);

            PutObjectRequest request = new PutObjectRequest();

            request.BucketName = bucketName;
            request.ContentType = ("applicaton/pdf");
            request.Key = path;
            request.StorageClass = S3StorageClass.ReducedRedundancy; //set storage to reduced redundancy
            request.InputStream = file.InputStream;

            try
            {
                S3Response uploadResponse = client.PutObject(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }

            return View("Refresh");

        }

        [HttpPost]

        public ActionResult DeletingAWS(string keyName)
        {
            if (!checkRequiredFields())
            {
                ViewBag.Result = "";
                return View();
            }
            try
            {

                DeleteObjectRequest request = new DeleteObjectRequest();
                request.WithBucketName(bucketName)
                    .WithKey(keyName);
                AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.APSoutheast1);
                DeleteObjectResponse response = client.DeleteObject(request);

                //using (DeleteObjectResponse response = client.DeleteObject(request))
                //{
                //    WebHeaderCollection headers = response.Headers;
                //    foreach (string key in headers.Keys)
                //    {
                //        Console.WriteLine("Response Header: {0}, Value: {1}", key, headers.Get(key));
                //    }
                //}
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An error occurred with the message '{0}' when deleting an object", amazonS3Exception.Message);
                }

                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Error removing the file from our File System, this file record is now removed");
            }
            return View("Refresh");
        }


        public void downloadAWS(int id, string path, string filename)
        {
            string keyName = path + "/" + filename;

            if (!checkRequiredFields())
            {
                ViewBag.Result = "";

            }
            AmazonS3 client;
            try
            {
                using (client = Amazon.AWSClientFactory.CreateAmazonS3Client())
                {
                    GetObjectRequest request = new GetObjectRequest().WithBucketName(bucketName).WithKey(keyName);

                    //string dest = ("C:\\user\\Downloads\\" + path + "\\" + filename);

                    string dest = HttpContext.Server.MapPath("~/App_Data/Downloads/" + id + '-' + filename);
                    using (GetObjectResponse response = client.GetObject(request))
                    {
                        response.WriteResponseStreamToFile(dest, false);

                        HttpContext.Response.Clear();
                        HttpContext.Response.AppendHeader("content-disposition", "attachment; filename=" + filename);
                        HttpContext.Response.ContentType = response.ContentType;
                        HttpContext.Response.TransmitFile(dest);
                        HttpContext.Response.Flush();
                        HttpContext.Response.End();
                    }


                    //Clean up temporary file.
                    //System.IO.File.Delete(dest);

                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An error occurred with the message '{0}' when reading an object", amazonS3Exception.Message);
                }


            }

        }
        #endregion


        #region Uploads  Documents
        [AcceptVerbs(HttpVerbs.Get)]
        public ViewResult UploadFileTemp(int id, int uni_Id)
        {
            if (TempData[SUCCESS_EDIT] != null)
            {
                ViewBag.SuccessEdit = true;
            }

            ViewBag.Course_Id = id;
            ViewBag.Uni_Id = uni_Id;

            return View();
        }


        [HttpPost]
        public ActionResult UploadFileTemp(FormCollection fc, HttpPostedFileBase file)
        {

            int CourseId = Convert.ToInt32(fc["Course_Id"]);
            int UniId = Convert.ToInt32(fc["Uni_Id"]);
            string comment = (fc["comment"]);
            string formname = (fc["formname"]);
            Template_Document appDoc = new Template_Document();

            if (file != null)
            {
                string pathToCreate;

                pathToCreate = TEMPLATE_APPLICATION_PATH + UniId + '_' + CourseId;

                // create / update folder 
                fileName = Path.GetFileName(file.FileName);
                //path = Path.Combine(Server.MapPath(pathToCreate), fileName);
                //file.SaveAs(path);


                string fileToCreate = pathToCreate + '/' + file.FileName;
                //appDoc.Path = Path.Combine(Server.MapPath(pathToCreate));
                appDoc.Path = pathToCreate;
                uploadAWS(fileToCreate, file);
            }
            else // Template with no downloadable form
            {
                fileName = "No File";
            }
            // Update object of application document file
            appDoc.Course_Id = CourseId;
            appDoc.Form_Name = formname;

            appDoc.UploadedOn = System.DateTime.Now;
            appDoc.FileName = fileName;
            appDoc.UploadedBy = CookieHelper.Username;
            appDoc.Comment = comment;



            if (ModelState.IsValid)
            {
                db.Template_Document.AddObject(appDoc);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Uploaded file To   " + appDoc.TemplateDoc_Id), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Template was uploaded successfully!");
                return View("Refresh");

            }
            //TempData[SUCCESS_EDIT] = true;
            return RedirectToAction("ShowAllDocumentTemplates");

        }

        // This will implement both case and application documents
        [Authorize]
        public ViewResult UploadApplicationResult(int Doc_Id, int case_Id, string doctype, string studentName = "Student")
        {
            if (TempData[SUCCESS_EDIT] != null)
            {
                ViewBag.SuccessEdit = true;
            }
            ViewBag.Doc_Id = Doc_Id;
            ViewBag.case_Id = case_Id;
            ViewBag.studentName = studentName;
            ViewBag.doctype = doctype;

            return View();
        }
        // This will implement both case and application documents
        [HttpPost]
        [Authorize]
        public ActionResult UploadApplicationResult(FormCollection fc, HttpPostedFileBase file)
        {
            int Doc_Id = Convert.ToInt32(fc["Doc_Id"]);
            int case_Id = Convert.ToInt32(fc["case_Id"]);
            string Doc_Type = fc["doctype"];
            string studentName = fc["studentName"];
            string comment = (fc["comment"]);

            string messageSubject = "";
            string messageBody = "";

            if (file == null)
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "File is not selected!");
            }



            // ****************************************************************
            // create a new folder if folder does not exist for that student  *
            // ****************************************************************

            fileName = Path.GetFileName(file.FileName);
            string name = StormWeb.Helper.Utilities.getName(CookieHelper.Username);
            string pathToCreate;
            // select file name according to type

            pathToCreate = STUDENT_UPLOADS_PATH + case_Id + '_' + studentName;

            Application_Result appDoc = new Application_Result();
            Application app = db.Applications.Single(a => a.Application_Id == Doc_Id);
            // Update object of application Result file for Offer Letter
            if (fc["doctype"] == "OfferLetter")
            {
                appDoc.Type = DocumentController.OFFER_LETTER_TYPE;
                appDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_Offer_Letter" + '_' + Doc_Id + Path.GetExtension(fileName);
                app.Status = ApplicationController.ApplicationStatusType.Offer_Letter.ToString();
                //ApplicationController.statusUp(app.Application_Id);
                ViewBag.doc = "Offer Letter" + Doc_Id;

                messageSubject = "Offer Letter";
                messageBody = "Your offer letter for " + app.Course.Course_Name + " at " + app.Course.Faculty.University.University_Name + " is now available. Please go to Documents and under your application (" + app.Course.Course_Name + ") where you can find the download link.";
            }
            else if (fc["doctype"] == "CoE")
            {
                appDoc.Type = DocumentController.COE_TYPE;
                appDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_CoE" + '_' + Doc_Id + Path.GetExtension(fileName);
                app.Status = ApplicationController.ApplicationStatusType.CoE.ToString();
                //ApplicationController.statusUp(app.Application_Id);
                ViewBag.doc = "CoE" + Doc_Id;

                messageSubject = "Confirmation of Enrolment";
                messageBody = "Your confirmation of enrolment for " + app.Course.Course_Name + " at " + app.Course.Faculty.University.University_Name + " is now available. Please go to Documents and under your application (" + app.Course.Course_Name + ") where you can find the download link.";
            }
            else if (fc["doctype"] == "Acceptance")
            {
                appDoc.Type = DocumentController.ACCEPTANCE_TYPE;
                appDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_Accept" + '_' + Doc_Id + Path.GetExtension(fileName);
                //app.Status = ApplicationController.ApplicationStatusType.Acceptance.ToString();
                //ApplicationController.statusUp(app.Application_Id);
                ViewBag.doc = "Accept" + Doc_Id;
            }
            else if (fc["doctype"] == "CompletedAcceptance")
            {
                appDoc.Type = DocumentController.COMPLETED_ACCEPTANCE_TYPE;
                appDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_CompletedAccept" + '_' + Doc_Id + Path.GetExtension(fileName);
                app.Status = ApplicationController.ApplicationStatusType.Acceptance.ToString();
                //ApplicationController.statusUp(app.Application_Id);
                ViewBag.doc = "_CompletedAccept" + Doc_Id;
            }
            else
            {
                //Error
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Operation unsuccessful, please try again later.");
                return View("Refresh");
            }
            appDoc.Application_Id = Doc_Id;
            appDoc.UploadedOn = System.DateTime.Now;
            appDoc.UploadedBy = CookieHelper.Username;
            appDoc.Path = pathToCreate;

            // to save file name with the application Id
            path = Path.Combine(Server.MapPath(pathToCreate), appDoc.FileName);

            appDoc.Comment = comment;
            db.Application_Result.AddObject(appDoc);

            string fileToCreate = pathToCreate + '/' + appDoc.FileName;
            uploadAWS(fileToCreate, file);
            //db.ObjectStateManager.ChangeObjectState(app, EntityState.Modified);

            if (ModelState.IsValid)
            {

                //file.SaveAs(path);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Uploaded file To   " + ViewBag.doc), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Updated file    " + ViewBag.doc), LogHelper.LOG_UPDATE, LogHelper.SECTION_DOCUMENT);

                if (appDoc.Type == DocumentController.OFFER_LETTER_TYPE)
                {
                    Payment pay = new Payment();

                    pay.Application_Id = appDoc.Application_Id;

                    db.Payments.AddObject(pay);
                    db.SaveChanges();
                }

                if (messageSubject != "")
                {
                    MessageController.sendSystemMessage(app.Student.UserName, messageSubject, messageBody);
                }
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Document Was Uploaded Successfully!");

                PopulatePayment(app.Application_Id);

            }

            TempData[SUCCESS_EDIT] = "true";
            //return RedirectToAction("Index", new { message = "Successfully Uploaded" });
            return View("Refresh");
        }


        // This will implement both case and application documents
        [Authorize]
        public ViewResult UploadCaseDoc(int Doc_Id, int Template_Id, string doctype, int caseId)
        {
            if (TempData[SUCCESS_EDIT] != null)
            {
                ViewBag.SuccessEdit = true;
            }
            ViewBag.Doc_Id = Doc_Id;
            ViewBag.Template_Id = Template_Id;
            ViewBag.doctype = doctype;
            ViewBag.caseId = caseId;
            return View();
        }
        // this will implement both case and application documents
        [HttpPost]
        [Authorize]
        public ActionResult UploadCaseDoc(FormCollection fc, HttpPostedFileBase file)
        {
            int Doc_Id = Convert.ToInt32(fc["Doc_Id"]);
            int Template_Id = Convert.ToInt32(fc["Template_Id"]);
            string comment = (fc["comment"]);
            int caseId = Convert.ToInt32(fc["caseId"]);

            if (file == null)
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "File is not selected!");
            }



            // ****************************************************************
            // create a new folder if folder does not exist for that student  *
            // ****************************************************************

            fileName = Path.GetFileName(file.FileName);
            string name = StormWeb.Helper.Utilities.getName(CookieHelper.Username);
            path = STUDENT_UPLOADS_PATH;


            if (fc["doctype"] == "ApplicationDocument")
            {
                Application_Document appDoc = new Application_Document();

                appDoc.Application_Id = Doc_Id;
                ViewBag.doc = "Application Document" + Doc_Id;
                appDoc.TemplateDoc_Id = Template_Id;
                appDoc.UploadedOn = System.DateTime.Now;
                appDoc.UploadedBy = CookieHelper.Username;
                appDoc.Path = path + caseId + '_' + name;
                appDoc.Approved = false;
                appDoc.FileName = Path.GetFileNameWithoutExtension(file.FileName) + "_Application_" + appDoc.Application_Id + Path.GetExtension(file.FileName);
                appDoc.Comment = comment;

                string fileToCreate = appDoc.Path + '/' + appDoc.FileName;
                uploadAWS(fileToCreate, file);

                db.Application_Document.AddObject(appDoc);

            }
            else
                if (fc["doctype"] == "CaseDocument")
                {
                    CaseDocument caseDoc = db.CaseDocuments.Single(x => x.Case_Id == caseId && x.CaseDocTemplate_Id == Template_Id);
                    ViewBag.doc = "Case Document" + Doc_Id;
                    caseDoc.UploadedOn = System.DateTime.Now;
                    caseDoc.UploadedBy = CookieHelper.Username;
                    caseDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_Case" + Path.GetExtension(fileName);
                    caseDoc.Path = path + caseId + '_' + name;
                    caseDoc.Comment = comment;

                    string fileToCreate = caseDoc.Path + '/' + caseDoc.FileName;
                    uploadAWS(fileToCreate, file);

                    db.ObjectStateManager.ChangeObjectState(caseDoc, EntityState.Modified);

                }

            if (ModelState.IsValid)
            {
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Uploaded new document"), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Document Was Uploaded Successfully!");

                Case c = db.Cases.SingleOrDefault(x => x.Case_Id == caseId);

                if (c != null)
                {
                    MessageController.sendSystemMessage(c.Case_Staff.FirstOrDefault().Staff.UserName, "New document", CookieHelper.Name + " uploaded new document to his/her application.");
                }
            }


            TempData[SUCCESS_EDIT] = "true";
            //return RedirectToAction("Index", new { message = "Successfully Uploaded" });
            return View("Refresh");

            //    int Doc_Id =  Convert.ToInt32(fc["Doc_Id"]);
            //    int Template_Id = Convert.ToInt32(fc["Template_Id"]);
            //    string comment = (fc["comment"]);
            //    int caseId = Convert.ToInt32(fc["caseId"]);

            //    if (file== null)
            //    {
            //        NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "File is not selected!");
            //    }


            //    //file.InputStream;
            //    // ****************************************************************
            //    // create a new folder if folder does not exist for that student  *
            //    // ****************************************************************

            //    fileName = Path.GetFileName(file.FileName);
            //    string name = StormWeb.Helper.Utilities.getName(CookieHelper.Username);
            //    string pathToCreate = STUDENT_UPLOADS_PATH + caseId + '_' + name;

            //    if (Directory.Exists(Server.MapPath(pathToCreate)))
            //    {

            //    }
            //    else
            //    {
            //        Directory.CreateDirectory(Server.MapPath(pathToCreate));
            //    }


            //    // Update object of application document file
            //    if (fc["doctype"] == "ApplicationDocument")
            //    {
            //        Application_Document appDoc = new Application_Document();

            //        appDoc.Application_Id = Doc_Id;
            //        ViewBag.doc = "Application Document" + Doc_Id;
            //        appDoc.TemplateDoc_Id = Template_Id;
            //        appDoc.UploadedOn = System.DateTime.Now;
            //        appDoc.UploadedBy = CookieHelper.Username;
            //        appDoc.Path = pathToCreate;
            //        appDoc.Approved = false;

            //        // to save file name with the application Id
            //        appDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_Application_" + appDoc.Application_Id + Path.GetExtension(fileName);
            //        path = Path.Combine(Server.MapPath(pathToCreate), appDoc.FileName);

            //        appDoc.Comment = comment;
            //        db.Application_Document.AddObject(appDoc);

            //    }
            //    else
            //        if (fc["doctype"] == "CaseDocument")
            //    {
            //        CaseDocument caseDoc = db.CaseDocuments.Single(x => x.Case_Id == Doc_Id && x.CaseDocTemplate_Id == Template_Id);
            //        ViewBag.doc = "Case Document" + Doc_Id;
            //        caseDoc.UploadedOn = System.DateTime.Now;
            //        caseDoc.UploadedBy = CookieHelper.Username;
            //        caseDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_Case" + Path.GetExtension(fileName);
            //        caseDoc.Path = pathToCreate;
            //        path = Path.Combine(Server.MapPath(pathToCreate), caseDoc.FileName);
            //        caseDoc.Comment = comment;
            //        db.ObjectStateManager.ChangeObjectState(caseDoc, EntityState.Modified);

            //    }

            //    if (ModelState.IsValid)
            //    {

            //        file.SaveAs(path); 
            //        db.SaveChanges();
            //        LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Uploaded file To   " + ViewBag.doc), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

            //        NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Document Was Uploaded Successfully!");

            //    }


            //TempData[SUCCESS_EDIT] = "true";
            //return RedirectToAction("Index", new { message = "Successfully Uploaded" });
        }


        public string GetFileNameFromTemplate(int tempId)
        {
            return db.Template_Document.DefaultIfEmpty(null).SingleOrDefault(x => x.TemplateDoc_Id == tempId).FileName;
        }
        #endregion Uploads

        #region Downloads
        // The following is used to download the previously uploaded Offer Letter 
        // Type is used to determine to download from application ID or file ID
        [Authorize]
        public void DownloadOfferLetter(int id, string type = "id")
        {
            Application_Result appDoc = null;
            if (type == "id")
                appDoc = db.Application_Result.SingleOrDefault(x => x.Id == id && x.Type == OFFER_LETTER_TYPE);
            else if (type == "appId")
                appDoc = db.Application_Result.SingleOrDefault(x => x.Application_Id == id && x.Type == OFFER_LETTER_TYPE);

            if (appDoc == null)
                return;
            downloadAWS(appDoc.Id, appDoc.Path, appDoc.FileName);
            //string path = appDoc.Path;
            //string fileToDownload = appDoc.FileName;
            //string file = Path.Combine(path, fileToDownload);
            //string ext = Path.GetExtension(file);
            //string contentType = "application/doc/pdf";

            ////Parameters to file are
            ////1. The File Path on the File Server
            ////2. The content type MIME type
            ////3. The parameter for the file save by the browser

            //return File(file, contentType, appDoc.FileName);
        }
        // The following is used to download the previously uploaded CoE
        // Type is used to determine to download from application ID or file ID
        public void DownloadCoE(int id, string type = "id")
        {

            Application_Result appDoc = null;
            if (type == "id")
                appDoc = db.Application_Result.Single(x => x.Id == id && x.Type == COE_TYPE);
            else if (type == "appId")
                appDoc = db.Application_Result.Single(x => x.Application_Id == id && x.Type == COE_TYPE);

            if (appDoc == null)
                return;
            downloadAWS(appDoc.Id, appDoc.Path, appDoc.FileName);
            //string path = appDoc.Path;
            //string fileToDownload = appDoc.FileName;
            //string file = Path.Combine(path, fileToDownload);
            //string ext = Path.GetExtension(file);
            //string contentType = "application/doc/pdf";

            ////Parameters to file are
            ////1. The File Path on the File Server
            ////2. The content type MIME type
            ////3. The parameter for the file save by the browser

            //return File(file, contentType, appDoc.FileName);
        }

        // The following is used to download the Acceptance Form
        // Type is used to determine to download from application ID or file ID
        public void DownloadAcceptance(int id, string type = "id")
        {
            Application_Result appDoc = null;
            if (type == "id")
                appDoc = db.Application_Result.Single(x => x.Id == id && x.Type == ACCEPTANCE_TYPE);
            else if (type == "appId")
                appDoc = db.Application_Result.Single(x => x.Application_Id == id && x.Type == ACCEPTANCE_TYPE);

            if (appDoc == null)
                return;
            downloadAWS(appDoc.Id, appDoc.Path, appDoc.FileName);
        }

        // The following is used to download the completed acceptance form
        // Type is used to determine to download from application ID or file ID
        public void DownloadCompletedAcceptance(int id, string type = "id")
        {
            Application_Result appDoc = null;
            if (type == "id")
                appDoc = db.Application_Result.Single(x => x.Id == id && x.Type == COMPLETED_ACCEPTANCE_TYPE);
            else if (type == "appId")
                appDoc = db.Application_Result.Single(x => x.Application_Id == id && x.Type == COMPLETED_ACCEPTANCE_TYPE);

            if (appDoc == null)
                return;
            downloadAWS(appDoc.Id, appDoc.Path, appDoc.FileName);
        }


        [Authorize]
        public void DownloadTempDoc(int id)
        {
            //string path;
            //string fileToDownload;
            //string file;
            //string contentType;
            //try
            //{
            Template_Document temDoc = db.Template_Document.Single(x => x.TemplateDoc_Id == id);
            downloadAWS(temDoc.TemplateDoc_Id, temDoc.Path, temDoc.FileName);
            //path = temDoc.Path;
            //fileToDownload = temDoc.FileName;
            //file = Path.Combine(path, fileToDownload);
            //contentType = "application/doc/pdf";

            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser
            //return File(file, contentType, fileToDownload);

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("{0} Exception caught.File not found", e);
            //}
            //return null;

        }

        // The following is used to download the previously uploaded file in the application list
        [Authorize]
        public void DownloadAppDoc(int id)
        {
            Application_Document appDoc = db.Application_Document.Single(x => x.ApplicationDoc_Id == id);

            downloadAWS(appDoc.ApplicationDoc_Id, appDoc.Path, appDoc.FileName);
            //string path = appDoc.Path;
            //string fileToDownload = appDoc.FileName;
            //string file = Path.Combine(path, fileToDownload);
            //string ext = Path.GetExtension(file);
            //string contentType = "application/doc/pdf";

            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser

            //return File(file, contentType, appDoc.FileName);
            //downloadAWS(path + '/' + fileToDownload);

        }


        public void DownloadCaseDocTemp(int id)
        {
            CaseDoc_Template casetemp = db.CaseDoc_Template.Single(x => x.CaseDocTemplate_Id == id);

            downloadAWS(casetemp.CaseDocTemplate_Id, casetemp.Path, casetemp.FileName);
            //string path = casetemp.Path;
            //string fileToDownload = casetemp.FileName;
            //string file = Path.Combine(path, fileToDownload);
            //string ext = Path.GetExtension(file);
            //string contentType = "application/doc/pdf";

            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser

            //return File(file, contentType, casetemp.FileName);
        }

        public void DownloadCaseDoc(int id)
        {
            CaseDocument caseDoc = db.CaseDocuments.Single(x => x.CaseDocument_Id == id);
            downloadAWS(caseDoc.CaseDocument_Id, caseDoc.Path, caseDoc.FileName);
            //string path = caseDoc.Path;
            //string fileToDownload = caseDoc.FileName;
            //string file = Path.Combine(path, fileToDownload);
            //string ext = Path.GetExtension(file);
            //string contentType = "application/doc/pdf";

            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser

            //return File(file, contentType, caseDoc.FileName);
        }

        #endregion Downloads

        #region Helper Methods

        public static bool isAllApplicationDocumentsUploaded(int applicationID)
        {
            StormDBEntities db = new StormDBEntities();
            string appStatus = db.Applications.Single(a => a.Application_Id == applicationID).Status;
            //ApplicationController.statusUp(applicationID);
            int progressValue = (int)Enum.Parse(typeof(StormWeb.Controllers.ApplicationController.ApplicationStatusType), appStatus);

            if (progressValue >= ApplicationController.getProgressValue(ApplicationController.ApplicationStatusType.Documents_Completed.ToString()))
                return true;

            return false;
        }

        public static int getNumberOfDownloadable(int applicationID)
        {


            return 0;
        }

        public static bool isOfferLetterRead(int applicationID)
        {
            StormDBEntities db = new StormDBEntities();
            Application_Result appDoc = db.Application_Result.SingleOrDefault(x => x.Application_Id == applicationID && x.Type == "O");

            if (appDoc == null)
                return false;
            return true;
        }

        public static bool isCOEReady(int applicationID)
        {
            StormDBEntities db = new StormDBEntities();
            Application_Result appDoc = db.Application_Result.SingleOrDefault(x => x.Application_Id == applicationID && x.Type == "C");

            if (appDoc == null)
                return false;
            return true;
        }

        public static bool isAllCaseDocumentsUploaded(int caseID)
        {
            StormDBEntities db = new StormDBEntities();

            int count = db.CaseDocuments.Where(x => x.Case_Id == caseID && x.FileName != null).Count();

            if (count <= 0)
                return false;
            return true;
        }

        #endregion

        #region Unused

        [Authorize(Roles = "Super,Counsellor,Administrator")]
        public ActionResult CreateCourseSelection()
        {

            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name");
            ViewBag.Uni_Id = new SelectList(db.Universities, "University_Id", "University_Name");

            ViewBag.Countries = from u in db.Countries
                                select new SelectListItem
                                {
                                    Text = u.Country_Name,
                                    Value = SqlFunctions.StringConvert((double)u.Country_Id),
                                };

            return View();

        }


        // POST: Course selection method
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        [HttpPost]
        public void CreateCourseSelection(FormCollection fc)
        {
            if (fc["Country_Select"] == "")
            {
                ModelState.AddModelError("CountryError", "Please select a Country");
            }
            if (fc["University_Select"] == "")
            {
                ModelState.AddModelError("UnivesityError", "Please select a University");
            }
            if (fc["Course_Select"] == "")
            {
                ModelState.AddModelError("CourseError", "Please select a Course");
            }
            else
            {
                int uni = Convert.ToInt32(fc["University_Select"]);
                ViewBag.course = Convert.ToInt32(fc["Course_Select"]);

            }

            if (ModelState.IsValid)
            {
                ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name");
                ViewBag.Uni_Id = new SelectList(db.Universities, "University_Id", "University_Name");
                // populate countries
                ViewBag.Countries = from u in db.Countries
                                    select new SelectListItem
                                    {
                                        Text = u.Country_Name,
                                        Value = SqlFunctions.StringConvert((double)u.Country_Id),
                                    };
            }
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name");
            ViewBag.University_Id = new SelectList(db.Universities, "University_Id", "University_Name");
        }

        // Return the list of universities
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult GetUniversities(int countryID = -1)
        {
            if (countryID == -1)
                return Json(Enumerable.Empty<SelectListItem>());

            IEnumerable<SelectListItem> selectList = from f in db.Universities
                                                     where f.Country_Id == countryID
                                                     select new SelectListItem
                                                     {
                                                         Text = f.University_Name,
                                                         Value = SqlFunctions.StringConvert((double)f.University_Id)
                                                     };

            return Json(selectList);
        }

        // Return the list of courses
        [Authorize(Roles = "Super,Counsellor,Administrator")]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult GetCourses(int universityID = -1)
        {
            if (universityID == -1)
                return Json(Enumerable.Empty<SelectListItem>());

            IEnumerable<SelectListItem> selectList = from f in db.Faculties
                                                     from c in db.Courses
                                                     where f.University_Id == universityID
                                                        && f.Faculty_Id == c.Faculty_Id
                                                     select new SelectListItem
                                                     {
                                                         Text = c.Course_Name,
                                                         Value = SqlFunctions.StringConvert((double)c.Course_Id)
                                                     };

            return Json(selectList);
        }

        public ActionResult Create()
        {
            ViewBag.Case_Id = new SelectList(db.Cases, "Case_Id", "CreatedBy");
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name");
            ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Course_Choice");
            return View();
        }

        //
        // POST: /Document/Create

        //
        // GET: /Document/Edit/5

        public ActionResult Edit(int id)
        {
            Application application = db.Applications.Single(a => a.Application_Id == id);
            ViewBag.Case_Id = new SelectList(db.Cases, "Case_Id", "CreatedBy", application.Case_Id);
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name", application.Course_Id);
            ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Course_Choice", application.Student_Id);
            return View(application);
        }

        //
        // POST: /Document/Edit/5

        [HttpPost]
        public ActionResult Edit(Application application)
        {
            if (ModelState.IsValid)
            {
                db.Applications.Attach(application);
                db.ObjectStateManager.ChangeObjectState(application, EntityState.Modified);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Edit Application " + application.Application_Id), LogHelper.LOG_UPDATE, LogHelper.SECTION_DOCUMENT);

                return RedirectToAction("Index");
            }
            ViewBag.Case_Id = new SelectList(db.Cases, "Case_Id", "CreatedBy", application.Case_Id);
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name", application.Course_Id);
            ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Course_Choice", application.Student_Id);
            return View(application);
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        #endregion

        #region CODE
        public static string SUCCESS_BOOK = "SuccessfulBook";
        public static string SUCCESS_EDIT = "SuccessfulEdit";
        public static string NO_BOOK = "NoBook";
        public static string TEMPLATE_APPLICATION_PATH = "Templates/";
        public static string TEMPLATE_GENERAL_PATH = "Templates/General";
        public static string STUDENT_UPLOADS_PATH = "Student/Uploads/";

        public static string OFFER_LETTER_TYPE = "O";
        public static string COE_TYPE = "C";
        public static string ACCEPTANCE_TYPE = "A";
        public static string COMPLETED_ACCEPTANCE_TYPE = "F";

        #endregion

        private void BindCurrency(string selectedValue)
        {
            StormWeb.Helper.Enumclass Enumclass = new Enumclass();
            var currency = Enumclass.GetCurrency();
            ViewData["CurrencyValue"] = new SelectList(currency, "Value", "Text", selectedValue);
        }



        public void PopulatePayment(int applicationId)
        {
            ApplicationDocumentViewModel appsViewModel = new ApplicationDocumentViewModel();
            int studentId = 0;
            if (CookieHelper.isStudent())
                studentId = CookieHelper.getStudentId();
            var appls = db.Applications.Where(x => x.Student_Id == studentId);
            foreach (var item in appls)
            {
                List<Payment> payments = db.Payments.Where(x => x.Application_Id == item.Application_Id).ToList();
                appsViewModel.paymentTable = payments;
                if (payments.Count() <= 0)
                    BindCurrency("--Select--");
                foreach (var item1 in payments)
                {
                    BindCurrency(item1.Currency);
                }
            }
        }

        [HttpPost]
        public ActionResult ApprovePayment(int appID)
        {
            Payment pay = db.Payments.SingleOrDefault(x => x.Application_Id == appID);

            if (pay == null)
                return Json(new { data = "false" });

            pay.Approved_By = CookieHelper.Username;

            MessageController.sendSystemMessage(pay.Application.Case.Student.UserName, "Payment updated", "The payment information for your application: " + pay.Application.Course.Course_Name + " have been approved.");

            db.SaveChanges();

            ApplicationController.statusUp(appID);

            return Json(new { data = "true" });
        }
        
        //public ActionResult SubmitPayment(int id, FormCollection Fc)
        //{

        //    Payment payment = db.Payments.SingleOrDefault(x => x.Application_Id == id);
        //    payment.Payment_Method = Fc["Payment_Method"];
        //    payment.Date_Of_Payment = DateTime.Now;

        //    db.ObjectStateManager.ChangeObjectState(payment, EntityState.Modified);

        //    db.SaveChanges();


        //    return View();
        //}
        //[HttpPost]
        //public JsonResult SubmitPayment(string PaymentMethod, string ReceiptNo, string DateOfPayment, int id,HttpPostedFileBase file)
        //{

        //    Payment payment = db.Payments.SingleOrDefault(x => x.Application_Id == id);
        //    ReceiptFile receipt = new ReceiptFile();
        //    if (payment != null)
        //    {
        //        payment.Payment_Method = PaymentMethod;
        //        payment.Receipt_No = ReceiptNo;
        //        payment.Date_Of_Payment =Convert.ToDateTime(DateOfPayment); 
        //    }

        //    uploadAWS(ReceiptNo, file);
        //    //string file1= Path.GetFileNameWithoutExtension(ReceiptNo.Split('\\')[3]) + "_Application_" + payment.Application_Id + Path.GetExtension(file.FileName);
        //    //receipt.FileName = Path.GetFileNameWithoutExtension(file.FileName) + "_Application_" + payment.Application_Id + Path.GetExtension(file.FileName);

        //    db.ObjectStateManager.ChangeObjectState(payment, EntityState.Modified);

        //    db.SaveChanges();
        //    return Json(new { Success = true }, JsonRequestBehavior.AllowGet);

        //}
        public static string PAYMENT_UPLOADS_PATH = "Payments/";

        //string pathToCreate;
        //        pathToCreate = TEMPLATE_GENERAL_PATH;
        //        string fileToCreate = pathToCreate + '/' + file.FileName;
        //        casedoc_template.FileName = file.FileName;
        //        casedoc_template.Path = pathToCreate;


        //        uploadAWS(fileToCreate, file);

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Lists(FormCollection Fc, HttpPostedFileBase Receipt_No)
        {
            int id = Convert.ToInt32(Fc["Application_Id"]);
            string PaymentMethod = Fc["Payment_Method"];
            string Currency = Fc["Currency"];
            string Amount = Fc["Amount"];
            string DateOfPayment = Fc["Date_Of_Payment"];

            Payment payment = db.Payments.SingleOrDefault(x => x.Application_Id == id);
            Receipt_File receipt = new Receipt_File();
            if (payment != null)
            {
                payment.Payment_Method = Fc["Payment_Method"];
                payment.Date_Of_Payment = Convert.ToDateTime(DateOfPayment);
            }
            if (Receipt_No != null)
            {
                string name = StormWeb.Helper.Utilities.getName(CookieHelper.Username);
                string fileToCreate = PAYMENT_UPLOADS_PATH + CookieHelper.StudentId + "_" + name + '/' + Path.GetFileNameWithoutExtension(Receipt_No.FileName) + "_Payment_" + payment.Application_Id + Path.GetExtension(Receipt_No.FileName);

                receipt.PaymentId = payment.Id;
                receipt.FileName = Path.GetFileNameWithoutExtension(Receipt_No.FileName) + "_Payment_" + payment.Application_Id + Path.GetExtension(Receipt_No.FileName);
                receipt.Path = PAYMENT_UPLOADS_PATH + CookieHelper.StudentId + "_" + name;
                receipt.UploadedBy = CookieHelper.Username;
                receipt.UploadedOn = DateTime.Now;
                uploadAWS(fileToCreate, Receipt_No);
                db.Receipt_File.AddObject(receipt);
                db.SaveChanges();
            }
            db.ObjectStateManager.ChangeObjectState(payment, EntityState.Modified);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public static bool fileExists(int applicationId)
        {
            StormDBEntities db = new StormDBEntities();
            var receipt = (from pay in db.Payments
                           from rec in db.Receipt_File
                           where pay.Application_Id == applicationId && rec.PaymentId == pay.Id
                           select rec);

            if (receipt.Count() > 0)
                return true;
            else
                return false;

        }
        public static string GetFileName(int applicationId)
        {
            StormDBEntities db = new StormDBEntities();
            string fileName = "";
            var receipt = (from pay in db.Payments
                           from rec in db.Receipt_File
                           where pay.Application_Id == applicationId && rec.PaymentId == pay.Id
                           select rec);
            fileName = receipt.FirstOrDefault().Path + '/' + receipt.FirstOrDefault().FileName;

            return fileName;
        }
        public static int GetFileId(int applicationId)
        {
            StormDBEntities db = new StormDBEntities();
            int fileName = 0;
            var receipt = (from pay in db.Payments
                           from rec in db.Receipt_File
                           where pay.Application_Id == applicationId && rec.PaymentId == pay.Id
                           select rec);
            fileName = receipt.FirstOrDefault().File_Id;
            return fileName;
        }
        public void DownloadPaymentReceipt(int id)
        {

            Receipt_File receipt = db.Receipt_File.SingleOrDefault(x => x.File_Id == id);
            if (receipt != null)
                downloadAWS(receipt.File_Id, receipt.Path, receipt.FileName);
        }

        public ActionResult DeletePaymentFile(int id, string page)
        {
            Payment payment = db.Payments.SingleOrDefault(x => x.Application_Id == id);
            Receipt_File receipt = db.Receipt_File.SingleOrDefault(x => x.PaymentId == payment.Id);
            db.Receipt_File.DeleteObject(receipt);
            db.SaveChanges();
            DeletingAWS(receipt.Path + '/' + receipt.FileName);
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfully deleted the File " + receipt.FileName);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Deleted the Receipt File " + receipt.FileName), LogHelper.LOG_DELETE, LogHelper.SECTION_PAYMENT);
            string redirectpage = "";
            if (page == "document")
                redirectpage = "../Document/Index";
            else if (page == "payment")
                redirectpage = "../Payment/Index";
            return RedirectToAction(redirectpage);
        }

        public static string GetAcceptanceFileName(int applicationId)
        {
            StormDBEntities db = new StormDBEntities();
            string fileName = "";
            var result = (from app in db.Application_Result 
                           where app.Application_Id == applicationId && app.Type == ACCEPTANCE_TYPE
                           select app);
            if(result!=null)
                fileName = result.FirstOrDefault().Path + '/' + result.FirstOrDefault().FileName;

            return fileName;
        }
        public static int GetAcceptanceFileId(int applicationId)
        {
            StormDBEntities db = new StormDBEntities();
            int fileName = 0;
            var result = (from app in db.Application_Result
                          where app.Application_Id == applicationId && app.Type == ACCEPTANCE_TYPE
                          select app);
            if (result != null)
                fileName = result.FirstOrDefault().Id;
            return fileName;
        }
        public void DownloadAcceptanceform(int id)
        {

            Application_Result result = db.Application_Result.SingleOrDefault(x => x.Id == id);
            if (result != null)
                downloadAWS(result.Id, result.Path, result.FileName);
        }

    }




}