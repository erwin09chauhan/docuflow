using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using DocuFlow.Domain.Entities;
using DocuFlow.Domain.Enums;
using MediatR;

namespace DocuFlow.Application.Commands.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IJwtService _jwtService;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(request.TenantSlug, cancellationToken);
        if (tenant is null)
            return Result<AuthDto>.Failure("Tenant not found.");

        var existing = await _userRepository.GetByEmailAsync(request.Email, tenant.Id, cancellationToken);
        if (existing is not null)
            return Result<AuthDto>.Failure("A user with this email already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = User.Create(tenant.Id, request.Email, passwordHash, request.FirstName, request.LastName);

        await _userRepository.AddAsync(user, cancellationToken);

        var token = _jwtService.GenerateToken(user);

        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role, user.CreatedAt);
        var authDto = new AuthDto(token.AccessToken, token.ExpiresAt, userDto);

        return Result<AuthDto>.Success(authDto);
    }
}