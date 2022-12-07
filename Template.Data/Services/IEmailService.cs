namespace Template.Data.Services
{

    public interface IEmailService
    {
        bool SendMail(string subject, string body, string to, string from = null, bool asHtml=true);
        Task<bool> SendMailAsync(string subject, string body, string to, string from = null, bool asHtml=true);
    }
}

