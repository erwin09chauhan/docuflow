namespace DocuFlow.Application.Abstractions.Services;

public record TokenResult(
    string AccessToken,
    DateTime ExpiresAt
);

public record AuthResult(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt
);

public interface IJwtService
{
    TokenResult GenerateAccessToken(string userId, string email, string role, Guid tenantId);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiry();
}