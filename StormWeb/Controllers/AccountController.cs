// --------------------------------------------------------------------------------------------------------------------
// <summary>
// File Name    : AccountController.cs
// Created Date : 13/08/2011
// Created By   : Jofry HS
// Description  : All actions related to Account management.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using StormWeb.Models;
using StormWeb.Helper;

namespace StormWeb.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/LogOn
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LogOn()
        {
            // User was redirected here because of authorization section
            //if (User.Identity != null && User.Identity.IsAuthenticated)
            //return RedirectToAction("Unauthorized");

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Unauthorized", "Account");
            }

            string msg = Request.QueryString["message"];

            if (!string.IsNullOrEmpty(msg))
            {
                if (msg == "registration-success")
                {
                    ViewBag.NotifyRegistrationSuccess = true;
                }
            }

            if (!string.IsNullOrEmpty((string)TempData["ResetPassword"]))
            {
                ViewBag.NotifyPasswordReset = true;
            }

            if (!string.IsNullOrEmpty((string)TempData[BAD_LINK]))
            {
                ViewBag.NotifyBadLink = true;
            }

            if (!string.IsNullOrEmpty((string)TempData[LOG_OFF]))
            {
                ViewBag.NotifyLogOff = true;
            }

            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                MembershipUser mu = Membership.GetUser(model.UserName);

                if (mu != null)
                    CookieHelper.LastLogin = mu.LastLoginDate.ToString();

                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    mu.LastLoginDate = DateTime.Now;

                    // Adding roles to the CookieHelper
                    RolesHelper rolesHelper = new RolesHelper(Roles.GetRolesForUser(model.UserName));

                    // Adding username to the CookieHelper
                    CookieHelper.Username = model.UserName;

                    // One user can have multiple roles
                    string[] roles = Roles.GetRolesForUser(model.UserName);
                    string rolesString = "";
                    foreach (string s in roles)
                        rolesString += s + "|";

                    CookieHelper.Roles = rolesString;

                    // If it is student
                    if (rolesHelper.isStudent())
                    {
                        StormDBEntities db = new StormDBEntities();
                        Student student = db.Students.Single(x => x.UserName == model.UserName);

                        // Add student id to the cookie                                      
                        CookieHelper.StudentId = Convert.ToString(student.Student_Id);

                        // Add the student name to the cookie
                        Client client = db.Clients.Single(x => x.Client_Id == student.Client_Id);
                        CookieHelper.Name = client.GivenName + " " + client.LastName;

                        // Branch assignment
                        Branch branch = (from b in db.Branches
                                         from c in db.Clients
                                         where b.Branch_Id == c.Branch_Id && c.Client_Id == student.Client_Id
                                         select b).SingleOrDefault();
                        if (branch != null)
                            CookieHelper.AssignedBranch = Convert.ToString(branch.Branch_Id);
                    }
                    else // If staff
                    {
                        // Add Staff ID and Name
                        StormDBEntities db = new StormDBEntities();
                        Staff staff = db.Staffs.First(x => x.UserName == model.UserName);
                        CookieHelper.StaffId = Convert.ToString(staff.Staff_Id);
                        CookieHelper.Name = staff.FirstName + " " + staff.LastName;

                        LogHelper.writeToStaffLog(model.UserName, "Logs in from: " + HttpContext.Request.ServerVariables["REMOTE_ADDR"], "OTHER", "Log");
                        // Also add branch where the staff belong                        
                        var branches = from b in db.Branches
                                       from bs in db.Branch_Staff
                                       from s in db.Staffs
                                       where s.Staff_Id == staff.Staff_Id && s.Staff_Id == bs.Staff_Id && b.Branch_Id == bs.Branch_Id
                                       select b;

                        if (CookieHelper.isInRole("Super"))
                        {
                            branches = db.Branches;
                        }

                        int branchCount = branches.Count();

                        if (branchCount == 0)
                            ModelState.AddModelError("NoBranch", "The staff is not assigned to any branch. Contact administrator!");
                        else if (branchCount == 1)
                        {
                            CookieHelper.AssignedBranch = Convert.ToString(branches.First().Branch_Id);
                        }
                        else
                        {
                            Branch[] bArr = branches.ToArray();

                            string combinedBranches = Convert.ToString(bArr[0].Branch_Id);
                            for (int i = 1; i < bArr.Length; i++)
                            {
                                combinedBranches += "|" + bArr[i].Branch_Id;
                            }

                            CookieHelper.AssignedBranch = combinedBranches;
                        }
                    }

                    if (!ModelState.IsValid)
                    {
                        FormsAuthentication.SignOut();
                        CookieHelper.destroyAllCookies();
                        return View(model);
                    }
                    // Redirect authenticated User
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // Page to show unauthorized access
        public ActionResult Unauthorized()
        {
            return View();
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            CookieHelper.destroyAllCookies();

            LogHelper.writeToStaffLog(CookieHelper.Username, "Logs out from: " + HttpContext.Request.ServerVariables["REMOTE_ADDR"], "OTHER", "Log");

            TempData[LOG_OFF] = "true";
            return RedirectToAction("LogOn", "Account");
        }

        public JsonResult IsUsernameExist(string username)
        {
            JsonResult result = new JsonResult();

            if (Membership.GetUser(username) == null)
            {
                result.Data = true;
            }
            else
                result.Data = false;

            return result;
        }

        public ActionResult ResetPassword(string id)
        {
            if (id == null)
                return RedirectToAction("LogOn", "Account");

            StormDBEntities db = new StormDBEntities();

            Password_Reset reset = db.Password_Reset.DefaultIfEmpty(null).Where(x => x.secretCode == id && x.resetted == false).SingleOrDefault();

            // If the reset request with that ID is not found, then redirect
            if (reset == null)
            {
                ViewBag.ResetStatus = false;
                TempData[BAD_LINK] = "true";
                return RedirectToAction("LogOn", "Account");
            }

            MembershipUser user = Membership.GetUser(reset.username);
            string tempPassword = user.ResetPassword();

            return View(new ResetPasswordModel(tempPassword, reset.username, id));
        }

        [HttpPost]
        public ActionResult ResetPassword(FormCollection fc)
        {
            // Retrieve password from the form
            ResetPasswordModel resetModel = new ResetPasswordModel(fc["tempPassword"], fc["username"], fc["secretCode"]);
            resetModel.newPassword = fc["newPassword"];

            // Validate the username
            MembershipUser user = Membership.GetUser(resetModel.username);

            // CHange the password
            user.ChangePassword(resetModel.tempPassword, resetModel.newPassword);

            StormDBEntities db = new StormDBEntities();

            // Set the reset status to true
            Password_Reset reset = db.Password_Reset.DefaultIfEmpty(null).Where(x => x.secretCode == resetModel.secretCode).SingleOrDefault();
            reset.resetted = true;
            db.SaveChanges();

            TempData["ResetPassword"] = "true";
            return RedirectToAction("LogOn", "Account");
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ForgotPassword(string usernameOrEmail)
        {
            if (String.IsNullOrEmpty(usernameOrEmail))
            {
                Response.StatusCode = 500;
                Response.Write("Error creating new reset password link");

                return new JsonResult();
            }

            // Get the user based on username or email
            MembershipUser user = Membership.GetUser(usernameOrEmail);
            if (user == null)
            {
                string username = Membership.GetUserNameByEmail(usernameOrEmail);
                if (String.IsNullOrEmpty(username))
                {
                    Response.StatusCode = 500;
                    Response.Write("Error creating new reset password link");

                    return new JsonResult();
                }
                user = Membership.GetUser(username);
            }

            // No user found, return error
            if (user == null)
            {
                return new JsonResult()
                {
                    Data = new { success = false }
                };
            }

            // Generate random secret key
            StormDBEntities db = new StormDBEntities();
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var secretKey = new string(
                Enumerable.Repeat(chars, 32)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            Password_Reset reset = new Password_Reset();

            reset.RequestDateTime = DateTime.Now;
            reset.username = user.UserName;
            reset.resetted = false;
            reset.secretCode = secretKey;

            try
            {

                db.Password_Reset.AddObject(reset);
                db.SaveChanges();

                string emailBody = "Please use this link to reset your email: ";
                emailBody += Url.Action("ResetPassword", "Account", new { id = secretKey }, "http");

                EmailHelper.sendEmail(new System.Net.Mail.MailAddress(user.Email), "Password Reset", emailBody);

                return new JsonResult()
                {
                    Data = new { success = true }
                };
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);

                Response.StatusCode = 500;
                Response.Write("Error creating new reset password link");

                return new JsonResult();
            }
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            Console.WriteLine();
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(Request.Form["Username"], model.Password, model.Email, null, null, true, null, out createStatus);
                Roles.AddUserToRole(Request.Form["Username"], "Counsellor");
                Roles.AddUserToRole(Request.Form["Username"], "Student");

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        public ActionResult ActivateStaff(int id)
        {
            StormDBEntities db = new StormDBEntities();

            Staff s = db.Staffs.DefaultIfEmpty(null).SingleOrDefault(x => x.Staff_Id == id);

            string usernameToActivate = s.UserName;

            MembershipUser mu = Membership.GetUser(usernameToActivate);

            if (mu != null)
            {
                TempData[AccountController.BAD_LINK] = "true";
                return RedirectToAction("LogOn");
            }

            ActivateAccountModel model = new ActivateAccountModel();
            model.username = usernameToActivate;
            model.staffId = id;
            model.email = s.Email;

            return View(model);
        }

        [HttpPost]
        public ActionResult ActivateStaff(ActivateAccountModel model)
        {
            MembershipUser mu = Membership.GetUser(model.username);

            if (mu == null)
            {
                if (ModelState.IsValid)
                {
                    // Attempt to register the user
                    MembershipCreateStatus createStatus;
                    Membership.CreateUser(Request.Form["Username"], model.password, model.email, null, null, true, null, out createStatus);
                    //Roles.AddUserToRole(Request.Form["Username"], "Counsellor");

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        return RedirectToAction("LogOn");
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorCodeToString(createStatus));
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }

            TempData[AccountController.BAD_LINK] = true;
            return RedirectToAction("LogOn");
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        public static int getNumberOfStudentsOnline()
        {
            int count = 0;
            foreach (MembershipUser user in Membership.GetAllUsers())
            {
                if (user.IsOnline)
                {
                    if (Roles.IsUserInRole(user.UserName, "Student"))
                        count++;
                }
            }

            return count;
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        #region Message Codes

        public static string BAD_LINK = "BadLink";
        public static string LOG_OFF = "LogOff";

        #endregion
    }
}
