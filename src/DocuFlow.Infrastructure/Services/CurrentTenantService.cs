using DocuFlow.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;

namespace DocuFlow.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _tenantId;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            if (_tenantId.HasValue)
                return _tenantId;

            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst("tenantId")?.Value;

            if (claim is not null && Guid.TryParse(claim, out var tenantId))
                return tenantId;

            return null;
        }
    }

    public void SetTenant(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}