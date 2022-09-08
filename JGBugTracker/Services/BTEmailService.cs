using JGBugTracker.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JGBugTracker.Services
{
    public class BTEmailService : IEmailSender
    {
        #region Properties
        private readonly MailSettings _mailSettings;
        #endregion

        #region Constructor
        public BTEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        #endregion

        #region Send Email
        public async Task SendEmailAsync(string userEmail, string subject, string htmlMessage)
        {
            // Configuration Setup
            string configEmail = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email")!;
            string host = _mailSettings.Host ?? Environment.GetEnvironmentVariable("Host")!;
            int port = _mailSettings.Port != 0 ? _mailSettings.Port : int.Parse(Environment.GetEnvironmentVariable("Port")!);
            string password = _mailSettings.Password ?? Environment.GetEnvironmentVariable("Password")!;
            // Email Setup
            MimeMessage email = new();
            email.Sender = MailboxAddress.Parse(userEmail);
            email.To.Add(MailboxAddress.Parse(configEmail));
            email.Subject = subject;
            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            email.Body = builder.ToMessageBody();
            // Email Send
            try
            {
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(configEmail, password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion   
    }
}
