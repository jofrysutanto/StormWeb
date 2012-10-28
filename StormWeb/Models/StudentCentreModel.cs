using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace StormWeb.Models
{
    public partial class StudentCentreModel
    {  
        /// <summary>
        /// List<case> is used for both student page and profile page in StudentCentre
        /// List<Course> is for student page
        /// List<Client_Qualification> is for profile page
        /// </summary> 


        public List<Student> studentTable { get; set; }
        public List<Client> clientTable { get; set; }
         

        public List<Case> caseTable { get; set; }
        public List<Course> courseTable { get; set; }

        public List<Client_Children_Detail> clientChildrenDetail { get; set; }
        public List<Client_Qualification> clientQualificationTable { get; set; }
        public List<Client_SkillTest>  clientSkillTestTable{ get; set; }
        public List<Client_Spouse>  clientSpouse{ get; set; }
        public List<Client_StudyServiceDetails>  clientStudyServiceDetails{ get; set; } 
        public List<Client_Work_Experience> clientWorkExperience{ get; set; }

    }

}

