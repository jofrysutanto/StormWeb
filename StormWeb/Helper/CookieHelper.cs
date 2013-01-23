// --------------------------------------------------------------------------------------------------------------------
// <summary>
// File Name    : CookieHelper.cs
// Created Date : 14/08/2011
// Created By   : Jofry HS
// Description  : Static Cookie management class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Helper
{
    public static class CookieHelper
    {
        static string usernameCookieVar = "username";
        static string studentIdVar = "student_id";
        static string staffIdVar = "staff_id";
        static string rolesVar = "roles";
        static string nameVar = "name";
        static string assignedBranchVar = "branch";
        static string lastLogin = "last_login";

        public static string Username
        {
            get
            {
                if (HttpContext.Current.Request.Cookies.AllKeys.Contains(usernameCookieVar))
                {
                    return HttpContext.Current.Request.Cookies[usernameCookieVar].Value;
                }
                else
                    return string.Empty;
            }
            set
            {
                HttpCookie cookie = new HttpCookie(usernameCookieVar);
                cookie.Value = value;
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public static string Name
        {
            get
            {
                if (HttpContext.Current.Request.Cookies.AllKeys.Contains(nameVar))
                {
                    return HttpContext.Current.Request.Cookies[nameVar].Value;
                }
                else
                    return string.Empty;
            }
            set
            {
                HttpCookie cookie = new HttpCookie(nameVar);
                cookie.Value = value;
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public static string LastLogin
        {
            get
            {
                if (HttpContext.Current.Request.Cookies.AllKeys.Contains(lastLogin))
                {
                    return HttpContext.Current.Request.Cookies[lastLogin].Value;
                }
                else
                    return string.Empty;
            }
            set
            {
                HttpCookie cookie = new HttpCookie(lastLogin);
                cookie.Value = value;
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public static string Roles
        {
            get
            {
                if (HttpContext.Current.Request.Cookies.AllKeys.Contains(rolesVar))
                {
                    return HttpContext.Current.Request.Cookies[rolesVar].Value;
                }
                else
                    return string.Empty;
            }
            set
            {
                HttpCookie cookie = new HttpCookie(rolesVar);
                cookie.Value = value;
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public static string StudentId
        {
            get
            {
                if (HttpContext.Current.Request.Cookies.AllKeys.Contains(studentIdVar))
                {
                    return HttpContext.Current.Request.Cookies[studentIdVar].Value;
                }
                else
                    return string.Empty;
            }
            set
            {
                HttpCookie cookie = new HttpCookie(studentIdVar);
                cookie.Value = value;
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public static int getStudentId()
        {
            return Convert.ToInt32(StudentId);
        }

        public static string StaffId
        {
            get
            {
                if (HttpContext.Current.Request.Cookies.AllKeys.Contains(staffIdVar))
                {
                    return HttpContext.Current.Request.Cookies[staffIdVar].Value;
                }
                else
                    return string.Empty;
            }
            set
            {
                HttpCookie cookie = new HttpCookie(staffIdVar);
                cookie.Value = value;
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public static int getStaffId()
        {
            return Convert.ToInt32(StaffId);
        }
        
        public static string AssignedBranch
        {
            get
            {
                if (HttpContext.Current.Request.Cookies.AllKeys.Contains(assignedBranchVar))
                {
                    return HttpContext.Current.Request.Cookies[assignedBranchVar].Value;
                }
                else
                    return string.Empty;
            }
            set
            {
                HttpCookie cookie = new HttpCookie(assignedBranchVar);
                cookie.Value = value;
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public static bool isInRole(string role)
        {
            string [] rolesArray = Roles.Split('|');

            return rolesArray.Contains(role);
        }

        public static bool isStudent()
        {
            string[] rolesArray = Roles.Split('|');
            return rolesArray.Contains("Student");
        }

        public static bool isStaff()
        {
            string[] rolesArray = Roles.Split('|');
            return !(rolesArray.Contains("Student"));            
        }

        public static void destroyAllCookies()
        {
            if (HttpContext.Current.Request.Cookies.AllKeys.Contains(usernameCookieVar))
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[usernameCookieVar];
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            if (HttpContext.Current.Request.Cookies.AllKeys.Contains(studentIdVar))
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[studentIdVar];
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            if (HttpContext.Current.Request.Cookies.AllKeys.Contains(rolesVar))
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[rolesVar];
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            if (HttpContext.Current.Request.Cookies.AllKeys.Contains(staffIdVar))
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[staffIdVar];
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            if (HttpContext.Current.Request.Cookies.AllKeys.Contains(nameVar))
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[nameVar];
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            if (HttpContext.Current.Request.Cookies.AllKeys.Contains(assignedBranchVar))
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[assignedBranchVar];
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            if (HttpContext.Current.Request.Cookies.AllKeys.Contains(lastLogin))
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[lastLogin];
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }
    }
}