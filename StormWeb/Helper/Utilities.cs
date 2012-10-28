/**
 * Author: Jofry HS
 * Date: 19/08/2012
 * 
 * Utilities.cs
 * 
 * Class to help with repeated static classes such as DateTime conversion to SQL type
 * 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StormWeb.Models;
using StormWeb.Controllers;

namespace StormWeb.Helper
{
    public static class Utilities
    {
        public static StormDBEntities db = new StormDBEntities();

        // Return the formatted DateTime to follow our format
        /// <summary>
        /// Reformat the date given, with the default format rule as : dd-MM-yyyy
        /// into DateTime object
        /// </summary>
        /// <param name="toFormat">String of the date to be converted into DateTime instance</param>
        /// <param name="formatRule">Date format</param>
        /// <returns>DateTime instance</returns>
        public static DateTime formatDate(string toFormat, string formatRule = "dd-MM-yyyy")
        {
            if (toFormat == "")
                return new DateTime();
            return DateTime.ParseExact(toFormat, formatRule,
                                       System.Globalization.CultureInfo.InvariantCulture);
        }

        // Query the number of appointment for particular Staff with staffId
        public static int getNumberOfAppointment(int staffId)
        {
            DateTime current = DateTime.Now;

            var upcomingApp = from app in db.Appointments
                              where app.Confirmation == "Confirmed" && app.Staff_Id == staffId && app.AppDateTime >= current
                              select app;

            //var myStudentApp = db.Appointments.ToList().Where(x => (x.Staff_Id == staffId) && (x.Confirmation.Equals("Request for Approval")));

            if (upcomingApp == null || upcomingApp.Count() <= 0)
                return 0;

            return upcomingApp.Count();
        }

        // Query the number of students who submitted enquiries
        public static int getNumberOfNewEnquiries(int staffId)
        {
            List<Branch> branchList = BranchHelper.getBranchList(CookieHelper.AssignedBranch);
            var appointsNewRequest = from br in branchList
                                    from app in db.Appointments                                    
                                    where app.Case_Id == null && app.Confirmation == AppointmentController.APP_REQUEST_APPROVAL && app.Branch_Id == br.Branch_Id
                                         select app;

            return appointsNewRequest.Count();
            //return appointsNewRequest.Count();
        }

        // Query number of students just registered and doesn't have staff assigned to it
        public static int getNumberOfNewStudents(int staffId)
        {
            List<Branch> branchList = BranchHelper.getBranchList(CookieHelper.AssignedBranch);

            /*var studentsWithCases = from br in branchList
                                    from st in db.Students
                                    from cs in db.Cases
                                    where st.Client.Branch_Id == br.Branch_Id && st.Student_Id == cs.Student_Id
                                    select st;

            var studentWithoutCases = from br in branchList
                                      from st in db.Students
                                      where st.Client.Branch_Id == br.Branch_Id
                                      select st;
            */
           var result = from b in branchList
                        from c in db.Cases                                     
                        where !(from cs in db.Case_Staff
                                select cs.Case_Id).Contains(c.Case_Id) && b.Branch_Id == c.Branch_Id
                        select c;

            return result.Count();
        }

        // Given the username, determine whether is a student
        public static bool isStudent(string username)
        {
            StormWeb.Models.StormDBEntities db = new StormWeb.Models.StormDBEntities();

            StormWeb.Models.Student s = db.Students.DefaultIfEmpty(null).SingleOrDefault(x => x.UserName == username);
            if (s == null)
                return false;
            return true;
        }

        public static bool isStaff(string username)
        {
            StormWeb.Models.StormDBEntities db = new StormWeb.Models.StormDBEntities();

            StormWeb.Models.Staff s = db.Staffs.DefaultIfEmpty(null).SingleOrDefault(x => x.UserName == username);
            if (s == null)
                return false;
            return true;
        }

        // GIven the username, get the Full Name of the person
        public static string getName(string username)
        {
            StormWeb.Models.StormDBEntities db = new StormWeb.Models.StormDBEntities();
            if (isStudent(username))
            {
                StormWeb.Models.Client client = (from c in db.Clients
                                                 from s in db.Students
                                                 where c.Client_Id == s.Client_Id && s.UserName == username
                                                 select c).Single();

                return client.GivenName + " " + client.LastName;

            }
            else if (isStaff(username))
            {
                StormWeb.Models.Staff staff = db.Staffs.Single(x => x.UserName == username);

                return staff.FirstName + " " + staff.LastName;
            }
            else
            {
                return username + " is unrecognized";
            }
        }

        public static int countNewInbox()
        {
            StormDBEntities dbCon = new StormDBEntities();

            string username = CookieHelper.Username;

            int count = (from mt in dbCon.Message_To
                         from m in dbCon.Messages
                         where m.Id == mt.Message_Id && mt.UserTo == username && mt.HasRead == false && mt.Deleted == false
                         select mt).ToList().Count;

            return count;
        }

        public static int countUploadedApplicationDocument(int studentId, int applicationId)
        {
            
            List<Application_Document> app = (from a in db.Applications
                                              from t in db.Application_Document
                                              where a.Student_Id == studentId && a.Application_Id == t.Application_Id && a.Application_Id == applicationId
                                              select t).ToList();

            return app.Count;
        }

        /// <summary>
        /// Count the number of documents to be uploaded for particular course
        /// </summary>
        /// <param name="courseId">Target course ID</param>
        /// <returns></returns>
        public static int countCourseDocument(int courseId)
        {
            List<Template_Document> appDocTemplates = db.Template_Document.Where(c => c.Course_Id == courseId).ToList();

            return appDocTemplates.Count;
        }

        public static int countUploadedCaseDocument(int caseId)
        {
            return db.CaseDocuments.Where(x => x.Case_Id == caseId && x.UploadedOn != null).ToList().Count;
        }

        public static int countTotalCaseDocument(int caseId)
        {
            return db.CaseDocuments.Where(x => x.Case_Id == caseId).ToList().Count;
        }
    }
}