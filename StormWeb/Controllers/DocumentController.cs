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
using System.IO;
using StormWeb.Helper;
using System.Data.Objects.SqlClient;
using System.Web.Script.Serialization;

namespace StormWeb.Controllers
{
    public class DocumentController : Controller
    {
        private StormDBEntities db = new StormDBEntities();
        private string fileName;
        private string path;
        int stid = 0;
        int staff = 0;


        #region Councelor  page
        
        // GET: /CaseTemp/
        // This will show all  Case Document Templates from student page to select from them
        [Authorize(Roles = "Super")]
        public ViewResult ShowGeneralTemplates()
        {
            return View(db.CaseDoc_Template.ToList());
            //return View();
        }




        // This will show all  Case Document Templates from student page to select from them
        [Authorize(Roles = "Super")]
        public ViewResult ShowAllCaseTemplates(int id = 0 )
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


        [Authorize(Roles = "Super")]
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
                        LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Delete Case Template " + casedoc.CaseDocTemplate_Id), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

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
            return RedirectToAction("StudentDetails", new { id=id});
            
        }

        // GET: /CaseTemp/Create

        [Authorize(Roles = "Super")]
        public ActionResult CreateCaseTemp(int returnId=0)
        {
            ViewBag.clientId = returnId;
            return View();
        }

        //
        // POST: /CaseTemp/Create

        [HttpPost]
        [Authorize(Roles = "Super")]
        public ActionResult CreateCaseTemp(CaseDoc_Template casedoc_template, HttpPostedFileBase file, int returnId=0)
        {
            if (ModelState.IsValid)
            {
                string pathToCreate;

                pathToCreate = TEMPLATE_GENERAL_PATH;

                // create / update folder 
                if (Directory.Exists(Server.MapPath(pathToCreate)))
                {

                }
                else
                {
                    Directory.CreateDirectory(Server.MapPath(pathToCreate));
                }

                if (file != null)
                {
                    fileName = Path.GetFileName(file.FileName);
                    path = Path.Combine(Server.MapPath(pathToCreate), fileName);
                    file.SaveAs(path);
                }

                casedoc_template.Path = Path.Combine(Server.MapPath(pathToCreate));
                casedoc_template.FileName = fileName;
                casedoc_template.UploadedOn = System.DateTime.Now;
                casedoc_template.UploadedBy = CookieHelper.Username;
               
                db.CaseDoc_Template.AddObject(casedoc_template);

                db.SaveChanges();

                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Created Case Template " + casedoc_template.CaseDocTemplate_Id), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Template Was Created Successfully!");
            }
            if (returnId != 0)
            {
                return RedirectToAction("ShowAllCaseTemplates", new { id = returnId });
            }
            else
            {
                return RedirectToAction("ShowGeneralTemplates");
            }
            //return View(casedoc_template);
        }

        // GET: /CaseTemp/Edit/5
        [Authorize(Roles = "Super")]
        public ActionResult EditCaseTemp(int id, int returnId = 0)
        {
            ViewBag.clientId = returnId;
            CaseDoc_Template casedoc_template = db.CaseDoc_Template.Single(c => c.CaseDocTemplate_Id == id);
            return View(casedoc_template);
        }

        //
        // POST: /CaseTemp/Edit/5

        [HttpPost]
        public ActionResult EditCaseTemp(CaseDoc_Template casedoc_template, int returnId = 0 )
        {
            if (ModelState.IsValid)
            {
                db.CaseDoc_Template.Attach(casedoc_template);
                db.ObjectStateManager.ChangeObjectState(casedoc_template, EntityState.Modified);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Edit Case Template " + casedoc_template.CaseDocTemplate_Id), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

            }
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Template Was Modified Successfully!");
            if (returnId != 0)
            {
                return RedirectToAction("ShowAllCaseTemplates", new { id = returnId });
            }
            else
            {
                return RedirectToAction("ShowGeneralTemplates");
            }
        }


        [Authorize(Roles = "Super")]
        public ActionResult DeleteCaseTemp(int id, int returnId=0)
        {
            CaseDocument found = (from e in db.CaseDocuments
                                  where e.CaseDocTemplate_Id == id 
                                  select e).DefaultIfEmpty(null).FirstOrDefault();

            CaseDoc_Template casedoc_template = db.CaseDoc_Template.DefaultIfEmpty(null).SingleOrDefault(c => c.CaseDocTemplate_Id == id );

            if ((casedoc_template != null) && ( found == null))
            {

                db.CaseDoc_Template.DeleteObject(casedoc_template);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Delete Case Template " + casedoc_template.CaseDocTemplate_Id), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Template was deleted successfully!");
            }

            else
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Template cannot be deleted as it is used in another application!");
                
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
            int stid = db.Students.Single(x => x.Client_Id == id).Student_Id;
            int caseId = db.Cases.Single(x => x.Student_Id == stid).Case_Id;
            string name = db.Clients.Single(x => x.Client_Id == id).GivenName + " " + db.Clients.Single(x => x.Client_Id == id).LastName;
            var applications = db.Applications.ToList().Where(c => c.Student_Id == stid && (c.Completed == false || c.Completed == null));
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
        // return all case documents for a selected student
        [Authorize(Roles = "Super")]
        public List<CaseDocument> GetCaseItems(int id)
        {
            return db.CaseDocuments.Where(x => x.Case_Id == id).ToList();
        }


        // The following method is used to display the list of students of the login staff
        [Authorize(Roles = "Super")]
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
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Approve Application " + app.Application_Id), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

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
                    StormWeb.Controllers.ApplicationController.setStatus(app.Application_Id, 40);
                }
                    // else return application status to "staff assigned"
                else
                {
                    StormWeb.Controllers.ApplicationController.setStatus(app.Application_Id, 20);

                }
            
             }
            return RedirectToAction("StudentDetails", new { id = returnId });
        }


    #endregion

        #region Template Documents
        // document templates 
        [Authorize(Roles = "Super")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ViewResult ShowAllDocumentTemplates()
        {
           
            var docs = db.Courses.ToList();
            return View(docs);
        }


        [Authorize(Roles = "Super")]
        public ActionResult EditDocTemp(int id)
        {
            Template_Document template_document = db.Template_Document.Single(t => t.TemplateDoc_Id == id);
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name", template_document.Course_Id);
            return View(template_document);
        }

        //
        // POST: /Template/Edit/5

        [HttpPost]
        [Authorize(Roles = "Super")]
        public ActionResult EditDocTemp(Template_Document template_document)
        {
            if (ModelState.IsValid)
            {
                db.Template_Document.Attach(template_document);
                db.ObjectStateManager.ChangeObjectState(template_document, EntityState.Modified);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Edit Template Document  " + template_document.TemplateDoc_Id), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

            }
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name", template_document.Course_Id);
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Template Was Modified Successfully!");
            return RedirectToAction("ShowAllDocumentTemplates");
        }


        #endregion

        #region List Of  Applications' Documents- INDEX

        // The following method is used to display the list of documents for a specific user
        [AcceptVerbs(HttpVerbs.Get)]
        [Authorize]
        public ViewResult Index()
        {
            if ((string)TempData[SUCCESS_EDIT] == "true")
            {
                ViewBag.SuccessEdit = true;
            }


            if (CookieHelper.isStaff())
            {
                stid = Convert.ToInt32(CookieHelper.StaffId);
            }
            if (CookieHelper.isStudent())
            {
                stid = Convert.ToInt32(CookieHelper.StudentId);
                //ViewBag.EditMessage = Request.QueryString["message"];

            }
            ViewBag.Message = Request.QueryString["message"];

            var applications = db.Applications.ToList().Where(c => c.Student_Id == stid);

            //List<Template_Document> templates;

            int caseId = db.Cases.Single(x => x.Student_Id == stid).Case_Id;
           
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
        public ActionResult DeleteOfferLetter(int id, string doctype, int returnId)
        {
            if (ModelState.IsValid)
            {
                Application_Result application = db.Application_Result.Single(a => a.Id == id);
                Application app = db.Applications.Single(a => a.Application_Id == application.Application_Id);

                string completFileName = Server.MapPath(application.Path + '/' + application.FileName);
                System.IO.File.Delete(completFileName);

                db.Application_Result.DeleteObject(application);
                if (doctype == "OfferLetter")
                {
                    app.Status = "Application_Submitted";
                }
                else if (doctype == "CoE")
                {
                    app.Status = "Offer_Letter";
                }
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Delete Application Result  " +"-" + application.Id), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

            }

            return RedirectToAction("StudentDetails", new { id = returnId });
        }

        // GET: /DocTemp/Delete/5
        [Authorize(Roles = "Super")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult DeleteDocTemp(int id)
        {
            Template_Document template_document = db.Template_Document.Single(t => t.TemplateDoc_Id == id);

           if (getAppByTemplate(id)== null) 
           {
               //string completFileName = Server.MapPath(template_document.Path + '/' + template_document.FileName);
               //System.IO.File.Delete(completFileName);

                db.Template_Document.DeleteObject(template_document);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Delete Template Document  " + template_document.TemplateDoc_Id), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

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
                System.IO.File.Delete(completFileName);

                db.Application_Document.DeleteObject(application);

                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Delete Application Document  " + application.ApplicationDoc_Id), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);
                
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
                System.IO.File.Delete(completFileName);

                // modify database
                application.Path = null;
                application.UploadedOn = null;
                application.FileName = null;
                application.Comment = null;

                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Delete Case Document  " + application.CaseDocument_Id), LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);

            }

            return RedirectToAction("Index");
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
            string pathToCreate;
            Template_Document appDoc = new Template_Document();
            pathToCreate = TEMPLATE_APPLICATION_PATH + UniId + '_' + CourseId;

            // create / update folder 
            if (Directory.Exists(Server.MapPath(pathToCreate)))
            {

            }
            else
            {
                Directory.CreateDirectory(Server.MapPath(pathToCreate));
            }
            if (file.Equals(null))
            {
                ModelState.AddModelError("UploadFileTemp", " Please Select a file! ");
            }
            else
            {
                fileName = Path.GetFileName(file.FileName);
                path = Path.Combine(Server.MapPath(pathToCreate), fileName);
                file.SaveAs(path);

                // Update object of application document file
                appDoc.Course_Id = CourseId;
                appDoc.Form_Name = formname;
                appDoc.Path = Path.Combine(Server.MapPath(pathToCreate));
                appDoc.UploadedOn = System.DateTime.Now;
                appDoc.FileName = fileName;
                appDoc.UploadedBy = CookieHelper.Username;
                appDoc.Comment = comment;
            }

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
        public ViewResult UploadOfferLetter(int Doc_Id, int case_Id, string doctype, string studentName)
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
        public ActionResult UploadOfferLetter(FormCollection fc, HttpPostedFileBase file)
        {
            int Doc_Id = Convert.ToInt32(fc["Doc_Id"]);
            int case_Id = Convert.ToInt32(fc["case_Id"]);
            string Doc_Type = fc["doctype"];
            string studentName = fc["studentName"];
            string comment = (fc["comment"]);

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
           
            // create / update folder 
            if (Directory.Exists(Server.MapPath(pathToCreate)))
            {

            }
            else
            {
                Directory.CreateDirectory(Server.MapPath(pathToCreate));
            }

            Application_Result appDoc = new Application_Result();
            Application app = db.Applications.Single(a => a.Application_Id == Doc_Id);
            // Update object of application Result file for Offer Letter
            if (fc["doctype"] == "OfferLetter")
            {
                appDoc.Type = "O";
                appDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_Offer_Letter" + '_' + Doc_Id + Path.GetExtension(fileName);
                app.Status = "Offer_Letter";
                ViewBag.doc = "Offer Letter" + Doc_Id;
            }
            else
            {
                appDoc.Type = "C";
                appDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_CoE" + '_' + Doc_Id + Path.GetExtension(fileName);
                app.Status = "CoE";
                ViewBag.doc = "CoE" + Doc_Id;
            }
                appDoc.Application_Id = Doc_Id;
                appDoc.UploadedOn = System.DateTime.Now;
                appDoc.UploadedBy = CookieHelper.Username;
                appDoc.Path = pathToCreate;

            // to save file name with the application Id
                path = Path.Combine(Server.MapPath(pathToCreate), appDoc.FileName);

                appDoc.Comment = comment;
                db.Application_Result.AddObject(appDoc);
                //db.ObjectStateManager.ChangeObjectState(app, EntityState.Modified);

            if (ModelState.IsValid)
            {

                file.SaveAs(path);
                db.SaveChanges();
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Uploaded file To   " + ViewBag.doc), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);
                LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Updated file    " + ViewBag.doc), LogHelper.LOG_UPDATE, LogHelper.SECTION_DOCUMENT);

                string sys_message = "Your offer letter for " + app.Course.Course_Name + " at " + app.Course.Faculty.University.University_Name + " is now available. Please go to Documents and under your application (" + app.Course.Course_Name + ") where you can find the download link."; 

                MessageController.sendSystemMessage(app.Student.UserName, "Offer Letter received", sys_message);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Document Was Uploaded Successfully!");

            }

            TempData[SUCCESS_EDIT] = "true";
            return RedirectToAction("Index", new { message = "Successfully Uploaded" });
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
                int Doc_Id =  Convert.ToInt32(fc["Doc_Id"]);
                int Template_Id = Convert.ToInt32(fc["Template_Id"]);
                string comment = (fc["comment"]);
                int caseId = Convert.ToInt32(fc["caseId"]);

                if (file== null)
                {
                    NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "File is not selected!");
                }

                

                // ****************************************************************
                // create a new folder if folder does not exist for that student  *
                // ****************************************************************
                
                fileName = Path.GetFileName(file.FileName);
                string name = StormWeb.Helper.Utilities.getName(CookieHelper.Username);
                string pathToCreate = STUDENT_UPLOADS_PATH + caseId + '_' + name;
                
                if (Directory.Exists(Server.MapPath(pathToCreate)))
                {
                        
                }
                else
                {
                    Directory.CreateDirectory(Server.MapPath(pathToCreate));
                }


                // Update object of application document file
                if (fc["doctype"] == "ApplicationDocument")
                {
                    Application_Document appDoc = new Application_Document();

                    appDoc.Application_Id = Doc_Id;
                    ViewBag.doc = "Application Document" + Doc_Id;
                    appDoc.TemplateDoc_Id = Template_Id;
                    appDoc.UploadedOn = System.DateTime.Now;
                    appDoc.UploadedBy = CookieHelper.Username;
                    appDoc.Path = pathToCreate;
                    appDoc.Approved = false;

                    // to save file name with the application Id
                    appDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_Application_" + appDoc.Application_Id + Path.GetExtension(fileName);
                    path = Path.Combine(Server.MapPath(pathToCreate), appDoc.FileName);

                    appDoc.Comment = comment;
                    db.Application_Document.AddObject(appDoc);

                }
                else
                    if (fc["doctype"] == "CaseDocument")
                {
                    CaseDocument caseDoc = db.CaseDocuments.Single(x => x.Case_Id == Doc_Id && x.CaseDocTemplate_Id == Template_Id);
                    ViewBag.doc = "Case Document" + Doc_Id;
                    caseDoc.UploadedOn = System.DateTime.Now;
                    caseDoc.UploadedBy = CookieHelper.Username;
                    caseDoc.FileName = Path.GetFileNameWithoutExtension(fileName) + "_Case" + Path.GetExtension(fileName);
                    caseDoc.Path = pathToCreate;
                    path = Path.Combine(Server.MapPath(pathToCreate), caseDoc.FileName);
                    caseDoc.Comment = comment;
                    db.ObjectStateManager.ChangeObjectState(caseDoc, EntityState.Modified);
                   
                }

                if (ModelState.IsValid)
                {

                    file.SaveAs(path); 
                    db.SaveChanges();
                    LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (" Uploaded file To   " + ViewBag.doc), LogHelper.LOG_CREATE, LogHelper.SECTION_DOCUMENT);

                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Document Was Uploaded Successfully!");

                }

            
            TempData[SUCCESS_EDIT] = "true";
            return RedirectToAction("Index", new { message = "Successfully Uploaded" });
        }
        public string GetFileNameFromTemplate(int tempId)
        {
            return db.Template_Document.DefaultIfEmpty(null).SingleOrDefault(x => x.TemplateDoc_Id == tempId).FileName;
        }
        #endregion Uploads

        #region Downloads
        // The following is used to download the previously uploaded Offer Letter 
        [Authorize]
        public FileResult DownloadOfferLetter(int id)
        {
            Application_Result appDoc = db.Application_Result.Single(x => x.Application_Id == id && x.Type == "O");
            string path = appDoc.Path;
            string fileToDownload = appDoc.FileName;
            string file = Path.Combine(path, fileToDownload);
            string ext = Path.GetExtension(file);
            string contentType = "application/doc/pdf";

            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser

            return File(file, contentType, appDoc.FileName);
        }
        // The following is used to download the previously uploaded CoE
        public FileResult DownloadCoE(int id)
        {
            Application_Result appDoc = db.Application_Result.Single(x => x.Application_Id == id && x.Type == "C");
            string path = appDoc.Path;
            string fileToDownload = appDoc.FileName;
            string file = Path.Combine(path, fileToDownload);
            string ext = Path.GetExtension(file);
            string contentType = "application/doc/pdf";

            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser

            return File(file, contentType, appDoc.FileName);
        }


        [Authorize]
        public FileResult DownloadTempDoc(int id)
        {
            string path;
            string fileToDownload;
            string file;
            string contentType;
            try
            {
                Template_Document temDoc = db.Template_Document.Single(x => x.TemplateDoc_Id == id);
                path = temDoc.Path;
                fileToDownload = temDoc.FileName;
                file = Path.Combine(path, fileToDownload);
                contentType = "application/doc/pdf";

                //Parameters to file are
                //1. The File Path on the File Server
                //2. The content type MIME type
                //3. The parameter for the file save by the browser
                return File(file, contentType, fileToDownload);

            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.File not found", e);
            }
            return null;

        }

        // The following is used to download the previously uploaded file in the application list
        [Authorize]
        public FileResult DownloadAppDoc(int id)
        {
            Application_Document appDoc = db.Application_Document.Single(x => x.ApplicationDoc_Id == id);
            string path = appDoc.Path;
            string fileToDownload = appDoc.FileName;
            string file = Path.Combine(path, fileToDownload);
            string ext = Path.GetExtension(file);
            string contentType = "application/doc/pdf";

            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser

            return File(file, contentType, appDoc.FileName);
        }

        
        public FileResult DownloadCaseDocTemp(int id)
        {
            CaseDoc_Template casetemp = db.CaseDoc_Template.Single(x => x.CaseDocTemplate_Id == id);
            string path = casetemp.Path;
            string fileToDownload = casetemp.FileName;
            string file = Path.Combine(path, fileToDownload);
            string ext = Path.GetExtension(file);
            string contentType = "application/doc/pdf";

            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser

            return File(file, contentType, casetemp.FileName);
        }

        public FileResult DownloadCaseDoc(int id)
        {
            CaseDocument caseDoc = db.CaseDocuments.Single(x => x.CaseDocument_Id == id);
            string path = caseDoc.Path;
            string fileToDownload = caseDoc.FileName;
            string file = Path.Combine(path, fileToDownload);
            string ext = Path.GetExtension(file);
            string contentType = "application/doc/pdf";

            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser

            return File(file, contentType, caseDoc.FileName);
        }

        #endregion Downloads

        #region Unused

        [Authorize(Roles = "Super")]
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
        [Authorize(Roles = "Super")]
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
        [Authorize(Roles = "Super")]
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
        [Authorize(Roles = "Super")]
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
        public static string TEMPLATE_APPLICATION_PATH = "~/App_Data/Templates/";
        public static string TEMPLATE_GENERAL_PATH = "~/App_Data/Templates/General";
        public static string STUDENT_UPLOADS_PATH = "~/App_Data/Uploads/";
        #endregion
    }
       

         
}