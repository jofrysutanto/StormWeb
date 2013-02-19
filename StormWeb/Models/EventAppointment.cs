using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace StormWeb.Models
{
    public class EventAppointment
    {
        [Required]
        public string EventId { get; set; }

        [Required]
        public string Venue { get; set; }

        [Required]
        public string Date { get; set; }

        [Required]
        public string Heading { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        
        public string EventAddedBy { get; set; }

        [Required]
        public string AudienceType { get; set; }
        
    }
}