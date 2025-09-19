using backend.Config;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace backend.Services
{
    public class EmailService
    {
        private readonly EmailConfig _emailConfig;

        public EmailService(EmailConfig emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendInvitationEmailAsync(string vendorEmail, Guid token)
        {
            var apiKey = _emailConfig.SendGridApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("conshus@partner.waters.com", "Superchat");
            var to = new EmailAddress(vendorEmail);
            var subject = "You're Invited to Join Our Vendor Platform";
            
            var setupUrl = $"http://localhost:5378/setup-vendor/{token}";
            var plainTextContent = $"Please complete your registration by clicking this link: {setupUrl}";
            var htmlContent = $"<strong>Please complete your registration by clicking this link:</strong> <a href='{setupUrl}'>Complete Registration</a>";
            
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            await client.SendEmailAsync(msg);
        }
    }
}