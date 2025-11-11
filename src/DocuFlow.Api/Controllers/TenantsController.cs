using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;

    public TenantsController(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request, CancellationToken ct)
    {
        var tenant = Tenant.Create(request.Name, request.Slug, request.Plan);
        await _tenantRepository.AddAsync(tenant, ct);
        return Created($"api/tenants/{tenant.Id}", new { tenant.Id, tenant.Name, tenant.Slug });
    }

    [HttpGet("{slug}")]
    [Authorize]
    public async Task<IActionResult> GetTenant(string slug, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(slug, ct);
        if (tenant is null)
            return NotFound();
        return Ok(new { tenant.Id, tenant.Name, tenant.Slug, tenant.Plan, tenant.IsActive });
    }
}

public record CreateTenantRequest(string Name, string Slug, string Plan = "Free");