using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AWAQ_Web.Pages
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.office365.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("skulltula.mn35@outlook.com", "875natha1595.Q") // Change to your actual credentials
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("skulltula.mn35@outlook.com"), // Change to your actual email
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}
