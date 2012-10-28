using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class StudentStaffModel
    {
        public List<Case> caseTable { get; set; }
        public List<Case_Staff> case_StaffTable { get; set; }
        public List<Application> applicationTable { get; set; }
        public List<Application_Document> applicationDocumentTable { get; set; }


    }
}