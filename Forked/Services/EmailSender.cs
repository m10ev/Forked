using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Forked.Services.Interfaces;

namespace Forked.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
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
            string html = await File.ReadAllTextAsync(templatePath);

            // Replace all placeholders like {{PLACEHOLDER}}
            foreach (var pair in replacements)
            {
                html = html.Replace(pair.Key, pair.Value);
            }

            await SendEmailAsync(email, subject, html);
        }
    }
}
