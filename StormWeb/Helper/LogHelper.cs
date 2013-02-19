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

using System.Web;

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
        public static bool writeToStudentLog(int CaseID, string Comment, string Type, string Section)
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
                l.Author = CookieHelper.Username;
                log.Add(l);
            }
            
            return saveStudentLog(log);
        }

        public static bool writeToStudentLog(string[] username, string Comment, string Type, string Section)
        {
            List<Student_Log> log = new List<Student_Log>();

            foreach (string s in username)
            {
                Student_Log l = new Student_Log();
                l.Comment = Comment; l.Type = Type; l.Section = Section;
                l.DateTime = DateTime.Now; l.Deleted = false;
                l.UserName = s;
                l.Author = CookieHelper.Username; 
                log.Add(l);
            }

            return saveStudentLog(log);
        }

        public static bool writeToSystemLog(int CaseID, string Comment, string Type, string Section)
        {
            List<System_Log> log = new List<System_Log>();

            string studentUsername = db.Cases.SingleOrDefault(x => x.Case_Id == CaseID).Student.UserName;

            System_Log l = new System_Log();
            l.UserName = studentUsername; l.Comment = Comment; l.Type = Type; l.Section = Section;
            l.DateTime = DateTime.Now; l.Deleted = false;
            log.Add(l);

            foreach (Case_Staff s in db.Case_Staff.Where(x => x.Case_Id == CaseID).ToList())
            {
                l = new System_Log();
                l.UserName = s.Staff.UserName; l.Comment = Comment; l.Type = Type; l.Section = Section;
                l.DateTime = DateTime.Now; l.Deleted = false; 
                log.Add(l);
            }

            return saveSystemLog(log);
        }

        public static bool writeToSystemLog(string[] username, string Comment, string Type, string Section)
        {
            List<System_Log> log = new List<System_Log>();

            foreach (string s in username)
            {
                System_Log l = new System_Log();
                l.Comment = Comment; l.Type = Type; l.Section = Section;
                l.DateTime = DateTime.Now; l.Deleted = false;
                l.UserName = s; 
                log.Add(l);
            }

            return saveSystemLog(log);
        }

        public static bool writeToStaffLog(string username, string comment, string type, string section)
        {
            Staff_Log sl = new Staff_Log();

            sl.DateTime = DateTime.Now;
            sl.Type = type;
            sl.Section = section;
            sl.UserName = username;
            sl.Comment = comment;

            try
            {

                db.Staff_Log.AddObject(sl);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        private static bool saveStudentLog(Student_Log log)
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

        private static bool saveStudentLog(List<Student_Log> log)
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

        private static bool saveSystemLog(System_Log log)
        {
            try
            {
                db.System_Log.AddObject(log);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                return false;
            }

            return true;
        }

        private static bool saveSystemLog(List<System_Log> log)
        {
            try
            {
                foreach (System_Log l in log)
                    db.System_Log.AddObject(l);
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

        public static string getIPAddress(HttpRequest request)
        {
            string ip;
            try
            {
                ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrEmpty(ip))
                {
                    if (ip.IndexOf(",") > 0)
                    {
                        string[] ipRange = ip.Split(',');
                        int le = ipRange.Length - 1;
                        ip = ipRange[le];
                    }
                }
                else
                {
                    ip = request.UserHostAddress;
                }
            }
            catch { ip = null; }

            return ip; 
        }

        #region LOG_TYPE

        public static string LOG_CREATE = "CREATE";
        public static string LOG_UPDATE = "UPDATE";
        public static string LOG_DELETE = "DELETE";
        public static string LOG_OTHER = "OTHER";

        public static string SECTION_ACCOUNT = "Account";
        public static string SECTION_APPLICATION = "Application";
        public static string SECTION_APPOINTMENT = "Appointment";
        public static string SECTION_BRANCH = "Branch";
        public static string SECTION_DOCUMENT = "Document";
        public static string SECTION_PAYMENT = "Payment";
        public static string SECTION_PROFILE = "Profile";
        public static string SECTION_MESSAGE = "Message";
        public static string SECTION_UNIVERSITY = "University";
        public static string SECTION_FACULTY = "Faculty";
        public static string SECTION_COURSE = "Course";
        public static string SECTION_CLIENT = "Client";
        public static string SECTION_RESUME = "Resume";
        public static string SECTION_LEAVE = "Leave";

        #endregion
    }   
}