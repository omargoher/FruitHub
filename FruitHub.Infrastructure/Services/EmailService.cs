using System.Net;
using System.Net.Mail;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Options;
using FruitHub.Infrastructure.Interfaces;
using Microsoft.Extensions.Options;

namespace FruitHub.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _email;

    public EmailService(IOptions<EmailOptions> options)
    {
        _email = options.Value;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_email.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);
        
        using var client = new SmtpClient(_email.Host, _email.Port)
        {
            Credentials = new NetworkCredential(
                _email.Username,
                _email.Password
            ),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
    }
}