using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class StaffAppointmentListViewModel
    {
        
        
        public List<Appointment> caseStudentAppointments { get; set; }
        public List<Appointment> studentAppointments { get; set; }
        public List<Appointment> studentPreviousApp { get; set; }
        public List<Appointment> clientAppointments { get; set; }
        public List<Appointment> myStudentAppointment { get; set; }
        public List<Appointment> confirmedAppointments { get; set; }
        public List<Appointment> staffPreviousApp { get; set; }
    }
}