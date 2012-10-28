using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace StormWeb.Models
{
    public partial class StudentModel
    {
        public List<Case> caseTable { get; set; }
        public List<Course> courseTable { get; set; } 
    }

}

