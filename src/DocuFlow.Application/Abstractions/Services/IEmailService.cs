namespace DocuFlow.Application.Abstractions.Services;

public record EmailMessage(
    string To,
    string Subject,
    string Body
);

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}