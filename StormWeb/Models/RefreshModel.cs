using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class RefreshModel
    {
        public string url { get; set; }

        public RefreshModel(string url)
        {
            this.url = url;
        }
    }
}