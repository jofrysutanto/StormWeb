using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StormWeb.Models
{

        [MetadataType(typeof(CaseDoc_TemplateMD))]
        public partial class CaseDoc_Template
        {
            public class CaseDoc_TemplateMD
            {
                [Required(ErrorMessage = "Please enter name")]
                public string Name { get; set; }



            }
        }

    
}