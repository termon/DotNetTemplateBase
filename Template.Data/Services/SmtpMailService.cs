using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Template.Data.Services;

namespace Template.Data.Services;

public class SmtpMailService : IMailService
{
    private readonly string _from;
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
     private readonly bool _enableSsl = true;

    // appsettings.json section MailSettings contains mail configuration
    public SmtpMailService(IConfiguration config)
    {
        _from = config.GetSection("MailSettings")["FromAddress"] ?? string.Empty; //.GetValue<string>("FromAddress");
        _host = config.GetSection("MailSettings")["Host"] ?? string.Empty;
        _port = Int32.Parse((config.GetSection("MailSettings")["Port"] ?? "0"));
        _username = config.GetSection("MailSettings")["UserName"] ?? string.Empty;
        _password = config.GetSection("MailSettings")["Password"] ?? string.Empty;
        _enableSsl = config.GetSection("MailSettings")["EnableSsl"]?.ToLower() == "true";  
    }
    
    // send mail
    public bool SendMail(string subject, string body, string to, string from = null, bool asHtml = true)
    {
        // now configure smtp client
        var client = new SmtpClient(_host, _port)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_username, _password),
            EnableSsl = _enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        try
        {
            // construct the mail message
            var mail = new MailMessage
            {
                From = new MailAddress(from ?? _from),
                Subject = subject,
                Body = body,
                IsBodyHtml = asHtml,

            };
            mail.To.Add(to);
            
            // now send the mail message
            client.Send(mail);  
            return true;
        }
        catch (Exception)
        { 
            // could not send email
            return false;
        }
    }
    
    // Send Mail Asynchronously
    public async Task<bool> SendMailAsync(string subject, string body, string to, string from = null, bool asHtml = true)
    {
        // now configure smtp client 
        var client = new SmtpClient(_host, _port)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_username, _password),
            EnableSsl = _enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        try
        {
            // construct the mail message
            var mail = new MailMessage
            {
                From = new MailAddress(from ?? _from),
                Subject = subject,
                Body = body,
                IsBodyHtml = asHtml,
            };
   
            mail.To.Add(to);
            
            // now send the mail message asynchronously
            await client.SendMailAsync(mail);  // client.Send(from, to, subject, message);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

