using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace LoginRegisterV2
{
    public static class EmailHelper
    {
        public static bool SendOtpEmail(string recipientEmail, int otp, string subject, string bodyIntro)
        {
            try
            {
                string senderEmail = ConfigurationManager.AppSettings["SmtpSenderEmail"];
                string senderPassword = ConfigurationManager.AppSettings["SmtpSenderPassword"];

                MailMessage message = new MailMessage();
                message.From = new MailAddress(senderEmail);
                message.To.Add(recipientEmail);
                message.Subject = subject;
                message.Body = bodyIntro + "\n\nYour verification code is " + otp +
                               "\n\nIf you did not request this, you can ignore this email.";

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                smtp.Send(message);

                return true;
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                return false;
            }
        }
    }
}