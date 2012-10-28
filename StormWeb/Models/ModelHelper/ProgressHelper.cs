using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models.ModelHelper
{
    public class ProgressHelper
    {

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

        public static float GetCountCaseUploads(int caseId)
        {
            // to get progress of uploaded general documents 
            // we will divide the count of uploaded documents by 
            // the count of total no of required documents 

            StormDBEntities db = new StormDBEntities();
            // select all templates assigned to that student
            int templates = db.CaseDocuments.Where( doc => doc.Case_Id == caseId).Count();

            int uploads = (from d in db.CaseDocuments
                           where d.UploadedOn != null
                           && d.Case_Id == caseId 
                           select d).Count();

            if (templates != 0)
            {
                return (uploads * 100) / templates ;
               
            }
            return 0;

        }


    }
}