namespace Forked.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendTemplateEmailAsync(string email, string subject, string templatePath, Dictionary<string, string> replacements);
    }
}
