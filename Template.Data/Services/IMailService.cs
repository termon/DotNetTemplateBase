namespace Template.Data.Services
{

    public interface IMailService
    {
        bool SendMail(string subject, string body, string to, string from = null, bool asHtml=true);
        Task<bool> SendMailAsync(string subject, string body, string to, string from = null, bool asHtml=true);
    }
}

