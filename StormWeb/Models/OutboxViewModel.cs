using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class OutboxViewModel
    {
        public Message message { get; set; }        
        public string nameFrom { get; set; }
    }
}