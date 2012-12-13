using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StormWeb.Models
{
    [MetadataType(typeof(MessageMD))]
    public partial class Message
    {
    }

    public class MessageMD
    {
        [Required]
        public string UserFrom { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd MMMM yyyy, H:mm:ss}")]
        public DateTime TimeStamp { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string MessageContent { get; set; }
    }
}