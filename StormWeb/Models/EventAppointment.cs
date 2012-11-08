using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class EventAppointment
    {
        public Appointment appointment { get; set; }
        public Event events { get; set; }
        public Address address { get; set; }
        public Case cases { get; set; }
        public Client client { get; set; }
        public General_Enquiry gen { get; set; }
        public Student student { get; set; }
        public Staff staff { get; set; }

        public string add{ get; set; }
    }
}