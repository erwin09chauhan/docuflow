using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Commands.Documents;
using DocuFlow.Application.Queries.Documents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentTenantService _currentTenantService;

    public DocumentsController(IMediator mediator, ICurrentTenantService currentTenantService)
    {
        _mediator = mediator;
        _currentTenantService = currentTenantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId;
        if (!tenantId.HasValue)
            return Unauthorized();

        var result = await _mediator.Send(new GetDocumentsQuery(tenantId.Value, page, pageSize), ct);
        if (!result.IsSuccess)
            return BadRequest(new { result.Error });
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken ct)
    {
        var tenantId = _currentTenantService.TenantId;
        if (!tenantId.HasValue)
            return Unauthorized();

        var result = await _mediator.Send(new GetDocumentByIdQuery(id, tenantId.Value), ct);
        if (!result.IsSuccess)
            return NotFound(new { result.Error });
        return Ok(result.Value);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var tenantId = _currentTenantService.TenantId;
        if (!tenantId.HasValue)
            return Unauthorized();

        var result = await _mediator.Send(new GetDocumentStatsQuery(tenantId.Value), ct);
        if (!result.IsSuccess)
            return BadRequest(new { result.Error });
        return Ok(result.Value);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string schema, CancellationToken ct)
    {
        var tenantId = _currentTenantService.TenantId;
        if (!tenantId.HasValue)
            return Unauthorized();

        if (!Guid.TryParse(User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized();

        if (!Enum.TryParse<DocuFlow.Domain.Enums.ExtractionSchema>(schema, true, out var extractionSchema))
            extractionSchema = DocuFlow.Domain.Enums.ExtractionSchema.Invoice;

        var command = new UploadDocumentCommand(
            tenantId.Value,
            userId,
            file.FileName,
            file.ContentType,
            file.Length,
            file.OpenReadStream(),
            extractionSchema
        );

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(new { result.Error });
        return Created($"api/documents/{result.Value?.Id}", result.Value);
    }
}