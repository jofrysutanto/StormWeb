using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class MaterialViewModel
    {
        public List<Material> material { get; set; }
        public List<Material_Order> placeOrder { get; set; }
    }
}