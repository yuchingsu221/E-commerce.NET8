using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.Net.Http;

namespace YuChingECommerce.Utility {
    public class EmailSender : IEmailSender {
        public string SendGridSecret { get; set; }

        public EmailSender(IConfiguration _config) {
            SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey");
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage) {
            //logic to send email

            /* SendGridClient mail 無法使用
            var client = new SendGridClient(SendGridSecret);

            var from = new EmailAddress("yuchingsu221@gmail.com", "Bulky Book");
            var to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            return client.SendEmailAsync(message);
            */
          
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("yuchingsu221@gmail.com", "kyef kuvo ysyt tkkd"),
                    EnableSsl = true,
                };
            
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("yuchingsu221@gmail.com", "Bulky Book"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);
            try
            {
                return smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex) when (ex is Exception)
            {
                LogUtility.LogInfo(string.Concat("發信發生失敗", Environment.NewLine, ex.Message));
            }
            return Task.FromResult(0);
        }
    }
}

