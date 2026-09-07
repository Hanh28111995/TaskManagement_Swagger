using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NewJira.Application.Interfaces.Services;

#nullable enable
namespace NewJira.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        string host = emailSettings["Host"] ?? "smtp.gmail.com";
        int port = int.TryParse(emailSettings["Port"], out var p) ? p : 587;
        string senderEmail = emailSettings["SenderEmail"] ?? string.Empty;
        string senderPassword = emailSettings["SenderPassword"] ?? string.Empty;

        var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, "NewJira System"),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}