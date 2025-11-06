using DocuFlow.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;

namespace DocuFlow.Infrastructure.Services;

public interface ICurrentTenantService
{
    Guid TenantId { get; }
}

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst("tenantId")?.Value;

            if (claim is null || !Guid.TryParse(claim, out var tenantId))
                throw new UnauthorizedAccessException("Tenant ID not found in token.");

            return tenantId;
        }
    }
}