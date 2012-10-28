using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StormWeb.Models.ModelHelper
{
    public class DocumentHelper
    {
        StormDBEntities db = new StormDBEntities();
        public Application_Document getDocFromTemplate(int tempId)
        {
            foreach (Application_Document ad in db.Application_Document)
            {
                if (ad.TemplateDoc_Id == tempId)
                {
                    return ad;
                }
            }

            return null;
        }

        public static CaseDocument hasCaseDoc(int caseId)
        {
            StormDBEntities db = new StormDBEntities();
            foreach (CaseDocument cd in db.CaseDocuments)
            {
                if (cd.Case_Id == caseId)
                {
                    return cd;
                }
            }

            return null;
        }

        public static float GetCountAppUploads(int courseId, int studentId)
        {
            // to get progress of uploaded documents for applications
            // we will divide the count of uploaded documents by 
            // the count of total no of required documents for each application

            StormDBEntities db = new StormDBEntities();
            int templates = db.Template_Document.Where(t => t.Course_Id == courseId).Count();
            int uploads = (from a in db.Applications
                            from d in db.Application_Document
                            where d.UploadedOn != null
                            && a.Student_Id == studentId
                            && a.Course_Id == courseId 
                            && a.Application_Id == d.Application_Id
                            select d).Count();
            if (templates != 0)
            {
                return (uploads * 100 / templates);
            }
            return 0;

        }

        // get number of approved documents to change application status to "Documents_Completed"
        public static int getUnApprovedDocs(int applicationId)
        {
            StormDBEntities db = new StormDBEntities();
            int countdocs = db.Application_Document.Where(d => d.Approved == false && d.Application_Id == applicationId).Count();
                             
            return countdocs;
        }


        public static void DeleteAppDocByStId(int stid)
        {
            StormDBEntities db = new StormDBEntities();
           
            
            // delete application Documents and corresponding files 
            var app = (from a in db.Applications
                       from d in db.Application_Document
                       where a.Application_Id == d.Application_Id && a.Student_Id == stid
                       select d).ToList();


            foreach (var application in app)
            {
                string completFileName = HttpContext.Current.Server.MapPath(application.Path + '/' + application.FileName);
                System.IO.File.Delete(completFileName);
                db.Application_Document.DeleteObject(application);
            }

            db.SaveChanges();


        }

        public  void DeleteCaseDocByStId(int stid)
        {
            StormDBEntities db = new StormDBEntities();

            // delete application Documents and corresponding files 
            var app = (from a in db.Applications
                       from c in db.CaseDocuments
                       where a.Case_Id == c.Case_Id && a.Student_Id == stid
                       select c).ToList();


            foreach (var application in app)
            {
                string completFileName = HttpContext.Current.Server.MapPath(application.Path + '/' + application.FileName);
                System.IO.File.Delete(completFileName);
                application.Path = null;
                application.UploadedOn = null;
                application.FileName = null;
                application.Comment = null;
            }

            db.SaveChanges();


        }


        public static void DeleteAppDocByAppId(int appId)
        {
            StormDBEntities db = new StormDBEntities();

            // delete application Documents and corresponding files 
            var app = (from a in db.Applications
                       from d in db.Application_Document
                       where a.Application_Id == d.Application_Id && a.Application_Id == appId
                       select d).ToList();


            foreach (var application in app)
            {
                string completFileName = HttpContext.Current.Server.MapPath(application.Path + '/' + application.FileName);
                System.IO.File.Delete(completFileName);
                db.Application_Document.DeleteObject(application);
            }

            db.SaveChanges();


        }

        public static void DeleteCaseDocByAppId(int appId)
        {
            StormDBEntities db = new StormDBEntities();

            // delete application Documents and corresponding files 
            var app = (from a in db.Applications
                       from c in db.CaseDocuments
                       where a.Case_Id == c.Case_Id && a.Application_Id == appId
                       select c).ToList();


            foreach (var application in app)
            {
                string completFileName = HttpContext.Current.Server.MapPath(application.Path + '/' + application.FileName);
                System.IO.File.Delete(completFileName);
                application.Path = null;
                application.UploadedOn = null;
                application.FileName = null;
                application.Comment = null;
            }

            db.SaveChanges();


        }



    }
}