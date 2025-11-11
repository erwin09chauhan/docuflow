using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Queries.Extraction;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ExtractionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentTenantService _currentTenantService;

    public ExtractionsController(IMediator mediator, ICurrentTenantService currentTenantService)
    {
        _mediator = mediator;
        _currentTenantService = currentTenantService;
    }

    [HttpGet("document/{documentId:guid}")]
    public async Task<IActionResult> GetExtractedFields(Guid documentId, CancellationToken ct)
    {
        var tenantId = _currentTenantService.TenantId;
        if (!tenantId.HasValue)
            return Unauthorized();

        var result = await _mediator.Send(new GetExtractedFieldsQuery(documentId, tenantId.Value), ct);
        if (!result.IsSuccess)
            return NotFound(new { result.Error });
        return Ok(result.Value);
    }
}