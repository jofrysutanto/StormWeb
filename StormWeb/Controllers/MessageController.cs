// --------------------------------------------------------------------------------------------------------------------
// <summary>
// File Name    : MessageController.cs
// Created Date : 24/08/2011
// Created By   : Jofry HS
// Description  : All actions related to Message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models; 
using StormWeb.Helper;
using StormWeb.Models.ModelHelper;

namespace StormWeb.Controllers
{
    public class MessageController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /Message/
        [Authorize(Roles="Student,Counsellor,Super,BranchManager")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            string UserName = "";
            if (CookieHelper.isStudent())
            {
                int StudentId = Convert.ToInt32(CookieHelper.StudentId);
                UserName = CookieHelper.Username;
                // Retrieve Contacts List
                ViewBag.ContactList = new SelectList(ContactHelper.GetContacts(true, StudentId), "Username", "Name");
            }
            else
            {
                // Is staff, get the ID
                int StaffId = CookieHelper.getStaffId();
                UserName = CookieHelper.Username;

                // Retrieve Contacts List
                ViewBag.ContactList = new SelectList(ContactHelper.GetContacts(false, StaffId), "Username", "Name");
                
                // ---- End of retrieve message list
            }

            ViewBag.InboxMessages = getInboxMessages(UserName);
            ViewBag.OutboxMessages = getOutBoxMessages(UserName);

            //Student student = db.Students.Single(x => x.Student_Id == StudentId);
            ViewBag.FromUsername = UserName;

            // ---- End of retrieve message list

            return View();
        }

         [Authorize(Roles = "Student,Counsellor,Super,BranchManager")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Compose()
        {
            string UserName;

            string UserTo = Request.QueryString["to"];
            string Layout = Request.QueryString["layout"];
            string closeBox = Request.QueryString["closeBox"];

            if (CookieHelper.isStudent())
            {
                int StudentId = Convert.ToInt32(CookieHelper.StudentId);
                UserName = CookieHelper.Username;
                // Retrieve Contacts List
                ViewBag.ContactList = new SelectList(ContactHelper.GetContacts(true, StudentId), "Username", "Name");
            }
            else
            {
                // Is staff, get the ID
                int StaffId = CookieHelper.getStaffId();
                UserName = CookieHelper.Username;

                // Retrieve Contacts List
                ViewBag.ContactList = new SelectList(ContactHelper.GetContacts(false, StaffId), "Username", "Name");

                // ---- End of retrieve message list
            }

            ViewBag.FromUsername = UserName;
            ViewBag.UserTo = UserTo;
            ViewBag.Layout = Layout;
            ViewBag.CloseBox = closeBox;

            return View(new Message());
        }

        [Authorize(Roles = "Student,Counsellor,Super,BranchManager")]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Compose(Message m, FormCollection fc)
        {
            //Message m = new Message();
            m.TimeStamp = DateTime.Now;
            m.Deleted = false;

 

            if (ModelState.IsValid)
            {
                if (fc["UserTo"] != "")
                {
                    db.Messages.AddObject(m);
                    db.SaveChanges();

                    int msgId = m.Id;

                    Message_To mt = new Message_To();
                    mt.Message_Id = msgId;
                    mt.UserTo = fc["UserTo"];
                    mt.HasRead = false;
                    mt.Deleted = false;

                    db.Message_To.AddObject(mt);
                    db.SaveChanges();

                    //TempData["MessageCompose"] = Compose_MessageSuccess;
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Your message has been sent!");

                    if (fc["Refresh"] != null)
                    {
                        return View("Refresh", new RefreshModel(Url.Action("Index", "Message")));
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Error processing your message");

                    if (fc["Refresh"] != null)
                    {
                        return View("Refresh", new RefreshModel(Url.Action("Index", "Message")));
                    }

                    return RedirectToAction("Index");
                }
            }

            return View("Index", m);
        }

       [Authorize(Roles = "Student,Counsellor,Super,BranchManager")]
        public ActionResult Inbox()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Read(int id, string type)
        {
            Message m = db.Messages.Single(x => x.Id == id);
            Message_To msgTo = db.Message_To.Single(x => x.Message_Id == id);

            if (type == "inbox")
            {
                msgTo.HasRead = true;
                db.SaveChanges();
                ViewBag.ReadType = "inbox";
                ViewBag.FromToName = Utilities.getName(m.UserFrom);
            }
            else if (type == "sent")
            {
                ViewBag.ReadType = "sent";
                ViewBag.FromToName = Utilities.getName(msgTo.UserTo);
            }
            return View(m);
        }

        [Authorize(Roles = "Student,Counsellor,Super,BranchManager")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult DeleteSent(int id)
        {
            ViewBag.ContactList = getContactList();
          
            Message m = db.Messages.Single( x => x.Id == id);
            m.Deleted = true;
            db.SaveChanges();

            return PartialView("~/Views/Message/Sent.cshtml", getOutBoxMessages(CookieHelper.Username));
        }

        [Authorize(Roles = "Student,Counsellor,Super,BranchManager")]
        public ActionResult DeleteInbox(int id)
        {
            ViewBag.ContactList = getContactList();

            Message_To mt = (from m in db.Messages
                            from message_to in db.Message_To
                             where m.Id == message_to.Message_Id && message_to.UserTo == CookieHelper.Username && m.Id == id
                             select message_to).DefaultIfEmpty(null).Single();

            if (mt != null)
            {
                //Message_To mt = db.Message_To.Single(x => x.Id == id && x.UserTo == CookieHelper.Username);
                mt.Deleted = true;
                db.SaveChanges();
            }           

            return PartialView("~/Views/Message/Inbox.cshtml", getInboxMessages(CookieHelper.Username));
        }

        public static void sendSystemMessage(string username, string subject, string content)
        {
            Message m = new Message();
            StormDBEntities db = new StormDBEntities();

            m.TimeStamp = DateTime.Now;
            m.Deleted = false;
            m.UserFrom = "SYSTEM";
            
            m.Subject = subject;
            m.MessageContent = content;

            db.Messages.AddObject(m);
            db.SaveChanges();

            Message_To mt = new Message_To();
            mt.UserTo = username;
            mt.HasRead = false;
            mt.Deleted = false;
            mt.Message_Id = m.Id;

            db.Message_To.AddObject(mt);
            db.SaveChanges();

            return;
        }

        // Return the list of contacts for the particular user based on their role and their id
        [Authorize(Roles = "Student,Counsellor,Super,BranchManager")]
        public SelectList getContactList()
        {
            if (CookieHelper.isStudent())
            {
                return new SelectList(ContactHelper.GetContacts(true, Convert.ToInt32(CookieHelper.StudentId)), "Username", "Name");
            }
            else
                return new SelectList(ContactHelper.GetContacts(false, Convert.ToInt32(CookieHelper.StaffId)), "Username", "Name");
        }
        [Authorize(Roles = "Student,Counsellor,Super,BranchManager")]
        public IEnumerable<InboxViewModel> getInboxMessages(string username)
        {
            // ---- Retrieve Message List
            // Retrieve Inbox
            var InboxMessages = from m in db.Messages
                                from mt in db.Message_To
                                where mt.UserTo == username && mt.Deleted == false && mt.Message_Id == m.Id
                                select m;
            IEnumerable<Message> inbox = InboxMessages.ToList();
            // We use the InboxViewModel to add 'HasRead' property from Message_To table
            List<InboxViewModel> inboxVMList = new List<InboxViewModel>();
            foreach (Message m in inbox)
            {
                InboxViewModel inboxVM = new InboxViewModel();
                inboxVM.message = m;
                inboxVM.hasRead = (bool)db.Message_To.Single(x => x.Message_Id == m.Id).HasRead;
                inboxVM.nameFrom = Utilities.getName(m.UserFrom);

                inboxVMList.Add(inboxVM);
            }
            return inboxVMList;
        }
        [Authorize(Roles = "Student,Counsellor,Super,BranchManager")]
        public IEnumerable<OutboxViewModel> getOutBoxMessages(string username)
        {
            List<OutboxViewModel> outboxVM = new List<OutboxViewModel>();

            // Retrieve Outbox
            var OutboxMessages = from m in db.Messages
                                 where m.UserFrom == username && m.Deleted == false
                                 select m;

            foreach (Message m in OutboxMessages)
            {
                OutboxViewModel outboxMsgModel = new OutboxViewModel();
                outboxMsgModel.message = m;
                Message_To mt = db.Message_To.Single(x => x.Message_Id == m.Id);
                outboxMsgModel.nameFrom = Utilities.getName(mt.UserTo);
                outboxVM.Add(outboxMsgModel);
            }

            return outboxVM;
        }

        #region Status Messages
        public static string Compose_MessageSuccess = "success-compose";
        #endregion
    }
}
