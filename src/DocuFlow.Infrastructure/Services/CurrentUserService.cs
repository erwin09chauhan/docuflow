using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DocuFlow.Infrastructure.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (claim is null || !Guid.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return userId;
        }
    }

    public string Email =>
        _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value
        ?? throw new UnauthorizedAccessException("Email not found in token.");

    public string Role =>
        _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Role)?.Value
        ?? throw new UnauthorizedAccessException("Role not found in token.");
}