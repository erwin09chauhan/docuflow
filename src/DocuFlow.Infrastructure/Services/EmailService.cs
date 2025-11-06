using DocuFlow.Application.Abstractions.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace DocuFlow.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var email = new MimeMessage();

        email.From.Add(MailboxAddress.Parse(
            _configuration["Email:FromAddress"] ?? "noreply@docuflow.com"));

        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;
        email.Body = new TextPart("html") { Text = message.Body };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _configuration["Email:SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost is not configured."),
            int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
            false,
            cancellationToken);

        await smtp.AuthenticateAsync(
            _configuration["Email:Username"] ?? throw new InvalidOperationException("Email:Username is not configured."),
            _configuration["Email:Password"] ?? throw new InvalidOperationException("Email:Password is not configured."),
            cancellationToken);

        await smtp.SendAsync(email, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}