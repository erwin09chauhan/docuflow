using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentTenantService _currentTenantService;

    public UsersController(
        IUserRepository userRepository,
        ICurrentTenantService currentTenantService)
    {
        _userRepository = userRepository;
        _currentTenantService = currentTenantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var tenantId = _currentTenantService.TenantId;
        if (!tenantId.HasValue)
            return Unauthorized();

        var users = await _userRepository.GetByTenantAsync(tenantId.Value, ct);
        var dtos = users.Select(u => new UserDto(
            u.Id,
            u.Email,
            u.FullName,
            u.Role,
            u.CreatedAt));

        return Ok(dtos);
    }
}