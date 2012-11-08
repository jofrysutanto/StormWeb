/**
 * AUthor: Jofry HS
 * Date: 20/09/2012
 * 
 * Send email, set to use GMail at current version (20/09/2012)
 * 
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace StormWeb.Helper
{
    public static class EmailHelper
    {
        /// <summary>
        /// Send an email using default Email Server configuration settings
        /// Email configuration can be found at Web.config
        /// </summary>
        /// <param name="to">Recipient of the email</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Email content</param>
        public static void sendEmail(MailAddress to, string subject, string body)
        {
            ServiceConfiguration config = new ServiceConfiguration();

            MailAddress fromAddress = new MailAddress(config.ServerEmail, "Storm Admin");
            string fromPassword = config.EmailPassword;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };
            using (var message = new MailMessage(fromAddress, to)
            {
                Subject = subject,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }

        private static string buildEmail(string body)
        {
            string result = "<html>";

            result += "<body>" + body + "</body>";

            result += "</html>";
            return result;
        }
    }
}