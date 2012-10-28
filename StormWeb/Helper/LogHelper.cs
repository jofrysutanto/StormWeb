// --------------------------------------------------------------------------------------------------------------------
// <summary>
// File Name    : LogHelper.cs
// Created Date : 23/08/2011
// Created By   : Jofry HS
// Description  : Static class to communicate with Log
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StormWeb.Models;
using System.Diagnostics;


namespace StormWeb.Helper
{
    public static class LogHelper
    {

        private static StormDBEntities db = new StormDBEntities();

        // Write a new log with supplied information
        /// <summary>
        /// Writes an entry to the log table
        /// </summary>
        /// <param name="CaseID">Student_Log's Case ID</param>
        /// <param name="Comment">Informative comment e.g. "Profile Update by xxx" or "Application deleted by xxx"</param>
        /// <param name="Type"> See LogHelper.LOG_* for list of type </param>
        /// <param name="Section"> See LogHelper.SECTION_* for list of sections </param>
        /// <returns>True if successful log write </returns>
        public static bool writeToLog(int CaseID, string Comment, string Type, string Section)
        {
            List<Student_Log> log = new List<Student_Log>();

            string studentUsername = db.Cases.SingleOrDefault(x => x.Case_Id == CaseID).Student.UserName;

            Student_Log l = new Student_Log();
            l.UserName = studentUsername; l.Comment = Comment; l.Type = Type; l.Section = Section;
            l.DateTime = DateTime.Now; l.Deleted = false;
            log.Add(l);

            foreach (Case_Staff s in db.Case_Staff.Where(x => x.Case_Id == CaseID).ToList())
            {
                l = new Student_Log();
                l.UserName = s.Staff.UserName; l.Comment = Comment; l.Type = Type; l.Section = Section;
                l.DateTime = DateTime.Now; l.Deleted = false;
                log.Add(l);
            }
            
            return saveLog(log);
        }

        public static bool writeToLog(string[] username, string Comment, string Type, string Section)
        {
            List<Student_Log> log = new List<Student_Log>();

            foreach (string s in username)
            {
                Student_Log l = new Student_Log();
                l.Comment = Comment; l.Type = Type; l.Section = Section;
                l.DateTime = DateTime.Now; l.Deleted = false;
                l.UserName = s;

                log.Add(l);
            }

            return saveLog(log);
        }

        private static bool saveLog(Student_Log log)
        {
            try
            {
                db.Student_Log.AddObject(log);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                return false;
            }

            return true;
        }

        private static bool saveLog(List<Student_Log> log)
        {
            try
            {
                foreach (Student_Log l in log)
                    db.Student_Log.AddObject(l);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                return false;
            }

            return true;
        }

        public static List<Student_Log> getLogsFromCookie()
        {
            string username = CookieHelper.Username;
            return db.Student_Log.Where(x => x.UserName == username && x.Deleted == false).ToList();
        }

        #region LOG_TYPE

        public static string LOG_CREATE = "CREATE";
        public static string LOG_UPDATE = "UPDATE";
        public static string LOG_DELETE = "DELETE";
        public static string LOG_OTHER = "OTHER";

        public static string SECTION_ACCOUNT = "Account";
        public static string SECTION_DOCUMENT = "Document";
        public static string SECTION_PROFILE = "Profile";
        public static string SECTION_APPOINTMENT = "Appointment";
        public static string SECTION_APPLICATION = "Application";
        public static string SECTION_MESSAGE = "Message";

        #endregion
    }   
}