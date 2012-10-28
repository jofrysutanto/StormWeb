using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StormWeb.Models
{
    [MetadataType(typeof(AppointmentMD))]
    public partial class Appointment
    {
    }

    public class AppointmentMD
    {       
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd MMMM yyyy, H:mm:ss}")]
        [Required(ErrorMessage="Please select an appointment date")]
        public DateTime AppDateTime { get; set; }

        //To check whether the MetaData class is checked, uncomment the following
        //If NotSetProperty exception is thrown, then this class is included
        //public string NotSetProperty { get; set; }
    }
}