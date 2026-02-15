using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Forked.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;

        public EmailSender(IConfiguration config, IWebHostEnvironment environment)
        {
            _config = config;
            _environment = environment;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["Smtp:From"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendTemplateEmailAsync(string email, string subject, string templatePath, Dictionary<string, string> replacements)
        {
            // Build the full path using the web root or content root
            var fullPath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", templatePath);

            string html = await File.ReadAllTextAsync(fullPath);

            foreach (var pair in replacements)
            {
                html = html.Replace(pair.Key, pair.Value);
            }

            await SendEmailAsync(email, subject, html);
        }
    }
}