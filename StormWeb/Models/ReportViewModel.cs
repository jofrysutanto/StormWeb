using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Foolproof;

namespace StormWeb.Models
{
    public class ReportViewModel
    {
        [Required(ErrorMessage = "Report Type is required")]
        [Display(Name = "Report Type")]
        public string reportType { get; set; }


        [Required(ErrorMessage = "Range is required")]
        [Display(Name = "Range")]
        public bool allTime { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy H:mm:ss}")]
        public DateTime dateTimeStart { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy H:mm:ss}")]
        public DateTime dateTimeEnd { get; set; }

        public int Branch { get; set; }

        public int Location { get; set; }

        public int Associate { get; set; }

        public int University { get; set; }

    }
}