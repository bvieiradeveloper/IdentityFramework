using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IdentityFramework.Configuration
{
    public class EmailService : IEmailSender
    {
        readonly string EMAIL_SENDER;
        readonly string EMAIL_PASSWORD_SENDER;
        public EmailService(IConfiguration configuration)
        {
            EMAIL_SENDER = configuration.GetValue<string>("EmailConfig:emailsender");
            EMAIL_PASSWORD_SENDER = configuration.GetValue<string>("EmailConfig:emailpasswordsender");

        }

        public EmailService()
        {

        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var emailMessage = new MailMessage())
            {
                emailMessage.From = new MailAddress(EMAIL_SENDER);
                emailMessage.To.Add(email);
                emailMessage.Subject = subject;
                emailMessage.Body = htmlMessage;

                using (var smtp = new SmtpClient())
                {
                    smtp.UseDefaultCredentials = false;

                    // if UseDefaultCredentials is true;
                    smtp.Credentials = new NetworkCredential(EMAIL_SENDER, password: EMAIL_PASSWORD_SENDER);

                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 25;
                    smtp.EnableSsl = true;
                    smtp.Timeout = 20_000;
                    smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    await smtp.SendMailAsync(emailMessage);
                }

            }
        }
    }
}
