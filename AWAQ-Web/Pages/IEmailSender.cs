using System;
namespace AWAQ_Web.Pages
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}

