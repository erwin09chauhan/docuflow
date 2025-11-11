using System.Security.Claims;
using DocuFlow.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;

namespace DocuFlow.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (claim is null || !Guid.TryParse(claim, out var userId))
                return null;

            return userId;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value;

    public string? Role =>
        _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Role)?.Value;
}