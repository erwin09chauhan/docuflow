using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Infrastructure.Services;
using System.Security.Claims;

namespace DocuFlow.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService currentTenantService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;

            if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                currentTenantService.SetTenant(tenantId);
            }
        }

        await _next(context);
    }
}