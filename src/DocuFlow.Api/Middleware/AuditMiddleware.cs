using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Domain.Entities;

namespace DocuFlow.Api.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICurrentUserService currentUserService,
        ICurrentTenantService currentTenantService,
        IAuditLogRepository auditLogRepository)
    {
        var mutatingMethods = new[] { "POST", "PUT", "DELETE", "PATCH" };

        await _next(context);

        if (context.User.Identity?.IsAuthenticated == true &&
            mutatingMethods.Contains(context.Request.Method))
        {
            var userId = currentUserService.UserId;
            var tenantId = currentTenantService.TenantId;

            if (userId.HasValue && tenantId.HasValue)
            {
                var log = AuditLog.Create(
                    tenantId.Value,
                    userId.Value,
                    context.Request.Method,
                    context.Request.Path.ToString(),
                    Guid.Empty,
                    $"{{\"statusCode\": {context.Response.StatusCode}}}");

                await auditLogRepository.AddAsync(log, CancellationToken.None);
            }
        }
    }
}