using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Commands.Auth;
using DocuFlow.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public AuthController(IMediator mediator, IAuthService authService, ICurrentUserService currentUserService,
    IUserRepository userRepository)
    {
        _mediator = mediator;
        _authService = authService;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(new { result.Error });
        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return Unauthorized(new { result.Error });
        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken, ct);
        if (!result.IsSuccess)
            return Unauthorized(new { result.Error });
        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RevokeAsync(request.RefreshToken, ct);
        if (!result.IsSuccess)
            return BadRequest(new { result.Error });
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId.Value, ct);
        if (user is null)
            return Unauthorized();

        return Ok(new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.CreatedAt));
    }
}

public record RefreshTokenRequest(string RefreshToken);