using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class DocumentIndexViewModel
    {
        public int courseId { get; set; }
        public List<Application_Document> appDoc { get; set; }
        public List<Template_Document> tempDoc { get; set; }

        public DocumentIndexViewModel(int courseId)
        {
            this.courseId = courseId;

            List<Application_Document> appDoc = new List<Application_Document>();
            List<Template_Document> tempDoc = new List<Template_Document>();
        }

        public Application_Document getDocFromTemplate(int tempId)
        {
            foreach (Application_Document ad in appDoc)
            {
                if (ad.TemplateDoc_Id == tempId)
                {
                    return ad;
                }
            }

            return null;
        }

        public Application_Document getCompletedDocFromTemplate(int tempId)
        {
            foreach (Application_Document ad in appDoc)
            {
                if (ad.TemplateDoc_Id == tempId && ad.UploadedOn != null)
                {
                    return ad;
                }
            }

            return null;
        }

        
    }
}