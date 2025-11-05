using DocuFlow.Domain.Entities;

namespace DocuFlow.Application.Abstractions.Services;

public record TokenResult(
    string AccessToken,
    DateTime ExpiresAt
);

public interface IJwtService
{
    TokenResult GenerateToken(User user);
}