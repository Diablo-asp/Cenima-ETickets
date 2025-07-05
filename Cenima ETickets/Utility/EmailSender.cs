using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Cinema_ETickets.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("di0ab0lo0@gmail.com", "ptfb eovl zkbj ubpg")
            };

            return client.SendMailAsync(
                new MailMessage(from: "di0ab0lo0@gmail.com",
                                to: email,
                                subject,
                                htmlMessage
                                )
                {
                    IsBodyHtml = true
                });
        }
    }
}
