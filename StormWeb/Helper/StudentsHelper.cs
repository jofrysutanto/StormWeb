/**
 * 
 * Created by Jofry HS
 * 09/07/2012
 * 
 * StudentsHelper
 * 
 * A class to help retrieving students list for a staff
 * 
 * 
 * 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StormWeb.Models;

namespace StormWeb.Helper
{
    public static class StudentsHelper
    {
        private static StormDBEntities db = new StormDBEntities();

        // Get the list of the students for the staff,
        // with the credentials in the cookie (StaffID/Username retrieved from Cookie)
        public static List<Student> getStudentsListFromCookie()
        {
            // Get StaffID
            int staffID = CookieHelper.getStaffId();

            return getStudentsList(staffID);
        }

        public static List<Student> getStudentsList(int staffID)
        {
            // Selecting students
            var students = from s in db.Students
                           from c in db.Cases
                           from cs in db.Case_Staff
                           where c.Student_Id == s.Student_Id && c.Case_Id == cs.Case_Id && cs.Staff_Id == staffID
                           select s;


            // Get the list of the students under this staff
            return students.ToList();
        }

        /// <summary>
        /// Validate if the Staff is actually assigned to the particular student
        /// </summary>
        /// <param name="staffID">Staff ID</param>
        /// <param name="studentID">Student ID</param>
        /// <returns>true or false</returns>
        public static bool staffAssignedToStudent(int staffID, int studentID)
        {
            var student = from s in db.Students
                          from c in db.Cases
                          from cs in db.Case_Staff
                          where c.Student_Id == s.Student_Id && c.Case_Id == cs.Case_Id && cs.Staff_Id == staffID && s.Student_Id == studentID
                          select s;

            if (student.ToList().Count <= 0)
                return false;
            return true;
        }

        public static string getStudentUsername(int studentID)
        {
            return db.Students.DefaultIfEmpty(null).SingleOrDefault(x => x.Student_Id == studentID).UserName;
        }

        public static bool hasCase(int studentID)
        {
            Case c = db.Cases.DefaultIfEmpty(null).SingleOrDefault(x => x.Student_Id == studentID);

            if (c == null)
                return false;

            return true;
        }

        public static Case getCase(int studentID)
        {
            Case c = db.Cases.DefaultIfEmpty(null).SingleOrDefault(x => x.Student_Id == studentID);

            return c;
        }
    }
}