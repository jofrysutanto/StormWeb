using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Helper
{
    public static class RoleType
    {
        public const string Student = "Student";
        public const string Counsellor = "Counsellor";
        public const string Admission = "Admission";
    }

    public class RolesHelper
    {
        private string[] roles;
        public RolesHelper(string [] s)
        {
            roles = s;
        }

        public bool isStudent()
        {
            return roles.Contains(RoleType.Student);
        }
    }
}