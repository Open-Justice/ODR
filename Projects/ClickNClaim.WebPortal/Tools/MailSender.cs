using ClickNClaim.Business;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace ClickNClaim.WebPortal.Tools
{
    public class MailSender
    {

        public static async Task SendMessage(string to, string subject, string body, Guid? guid = null)
        {
            try {
                if (guid != null && guid.HasValue)
                {
                    var email = BLLEmails.AddEmail(to, subject, body, guid.Value);
                }

                MailMessage mail = new MailMessage("contact@fast-arbitre.com", to);
                mail.From = new MailAddress("contact@fast-arbitre.com", "Fast-Arbitre");
                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                mail.IsBodyHtml = true;
                client.Credentials = new NetworkCredential("19ac047817bd68bafef91dd8de84a3c2", "a250417e22c62f5f49eaaa6f1c08b074");
                client.Host = "in-v3.mailjet.com";
                mail.Subject = subject;
                mail.Body = body;
                client.Send(mail);
            }
            catch(Exception e)
            {

            }
        }

        public static string GetHtmlAndReplaceData(string filename, Dictionary<string,string> data)
        {
            var str = File.OpenText(HttpContext.Current.Server.MapPath(filename)).ReadToEnd();
            foreach (var item in data)
            {
               str =  str.Replace(item.Key, item.Value);
            }
            return str;
        }

    }
}