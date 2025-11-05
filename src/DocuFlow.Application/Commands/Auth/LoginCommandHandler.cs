using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(request.TenantSlug, cancellationToken);
        if (tenant is null)
            return Result<AuthDto>.Failure("Invalid credentials.");

        var user = await _userRepository.GetByEmailAsync(request.Email, tenant.Id, cancellationToken);
        if (user is null)
            return Result<AuthDto>.Failure("Invalid credentials.");

        var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!validPassword)
            return Result<AuthDto>.Failure("Invalid credentials.");

        var token = _jwtService.GenerateToken(user);

        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role, user.CreatedAt);
        var authDto = new AuthDto(token.AccessToken, token.ExpiresAt, userDto);

        return Result<AuthDto>.Success(authDto);
    }
}