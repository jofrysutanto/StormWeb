using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class InboxViewModel
    {
        public Message message { get; set; }
        public bool hasRead { get; set; }
        public string nameFrom { get; set; }
    }
}