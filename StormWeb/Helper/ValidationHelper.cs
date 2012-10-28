using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StormWeb.Models;

namespace StormWeb.Helper
{
    public class ValidationHelper
    {
        private static StormDBEntities db = new StormDBEntities();

        /// <summary>
        /// Given the student id, check if the student exist in the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool isStudent(int id)
        {
            return db.Students.DefaultIfEmpty(null).Single(x => x.Student_Id == id) == null ? false : true;
        }

        /// <summary>
        /// Given the staff id, check if the staff exist in the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool isStaff(int id)
        {
            return db.Staffs.DefaultIfEmpty(null).Single(x => x.Staff_Id == id) == null ? false : true;
        }
    }
}