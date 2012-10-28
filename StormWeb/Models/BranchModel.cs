using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StormWeb.Models
{
    public partial class BranchModel
    {
        public List<Branch> branchTable { get; set; }
        public List<Address> addressTable { get; set; }
        public List<Country> countryTable { get; set; }
    }
}