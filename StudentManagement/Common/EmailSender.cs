
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;

namespace StudentManagement.Common
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("kidurshalini1026@gmail.com", "fpao todv cspf mmlt"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("kidurshalini1026@gmail.com"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            try
            {
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully!");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }

        }
    }

}



