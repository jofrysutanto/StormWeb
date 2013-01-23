using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace StormWeb.Models
{
    [MetadataType(typeof(AdvertisementMD))]
    public partial class Advertisement
    {
    }

    public class AdvertisementMD
    {
        [Required]
        public string Heading { get; set; }

        [Required]
        [AllowHtml]
        public string Comments { get; set; }

    }

    [MetadataType(typeof(Advertisement_FileMD))]
    public partial class Advertisement_File
    {
    }

    public class Advertisement_FileMD
    {
        [Required]
        public string UploadedBy { get; set; } 

    }
}