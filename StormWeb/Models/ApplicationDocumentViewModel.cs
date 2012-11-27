using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StormWeb.Controllers;

namespace StormWeb.Models
{
    public class ApplicationDocumentViewModel
    {
        public ApplicationDocumentViewModel()
        {
            app = new Application();

            completed = new List<Application_Document>();
            notCompleted = new List<Template_Document>();
            completedGeneralDocs = new List<CaseDocument>();
            notCompletedGeneralDocs = new List<CaseDocument>();

            offer = new Application_Result();
            acceptance = new Application_Result();
            completedAcceptance = new Application_Result();
            coe = new Application_Result();
        }

        public Application app { get; set; }

        public List<Application_Document> completed { get; set; }
        public List<Template_Document> notCompleted { get; set; }
        public List<CaseDocument> completedGeneralDocs { get; set; }
        public List<CaseDocument> notCompletedGeneralDocs { get; set; }

        public Application_Result offer { get; set; }
        public Application_Result acceptance { get; set; }
        public Application_Result completedAcceptance { get; set; }
        public Application_Result coe { get; set; }

        public bool step1Complete()
        {
            if (notCompletedGeneralDocs.Count <= 0 && notCompleted.Count <= 0)
                return true;
            return false;
        }

        public bool step2Complete()
        {
            if (ApplicationController.getApplicationStatusTypeValue(app.Status) >= ApplicationController.getApplicationStatusTypeValue(ApplicationController.ApplicationStatusType.Application_Submitted.ToString()))
            {
                return true;
            }
            return false;
        }

        public bool step3Complete()
        {
            if (ApplicationController.getApplicationStatusTypeValue(app.Status) >= ApplicationController.getApplicationStatusTypeValue(ApplicationController.ApplicationStatusType.Acceptance.ToString()))
                return true;
            return false;
        }

        public bool step4Complete()
        {
            if (ApplicationController.getApplicationStatusTypeValue(app.Status) >= ApplicationController.getApplicationStatusTypeValue(ApplicationController.ApplicationStatusType.CoE.ToString()))
                return true;
            return false;
        }
    }
}