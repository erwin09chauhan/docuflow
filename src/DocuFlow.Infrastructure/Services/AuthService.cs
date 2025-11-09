using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using DocuFlow.Infrastructure.Identity;
using DocuFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocuFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITenantRepository _tenantRepository;
    private readonly IJwtService _jwtService;
    private readonly AppDbContext _context;

    public AuthService(
        UserManager<AppUser> userManager,
        ITenantRepository tenantRepository,
        IJwtService jwtService,
        AppDbContext context)
    {
        _userManager = userManager;
        _tenantRepository = tenantRepository;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<Result<AuthDto>> LoginAsync(string tenantSlug, string email, string password, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(tenantSlug, ct);
        if (tenant is null)
            return Result<AuthDto>.Failure("Invalid credentials.");

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenant.Id, ct);
        if (user is null)
            return Result<AuthDto>.Failure("Invalid credentials.");

        var validPassword = await _userManager.CheckPasswordAsync(user, password);
        if (!validPassword)
            return Result<AuthDto>.Failure("Invalid credentials.");

        return Result<AuthDto>.Success(await GenerateAuthDtoAsync(user, ct));
    }

    public async Task<Result<AuthDto>> RegisterAsync(string tenantSlug, string email, string password, string firstName, string lastName, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(tenantSlug, ct);
        if (tenant is null)
            return Result<AuthDto>.Failure("Tenant not found.");

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            return Result<AuthDto>.Failure("A user with this email already exists.");

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            TenantId = tenant.Id,
            Role = Domain.Enums.UserRole.Viewer
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<AuthDto>.Failure(errors);
        }

        return Result<AuthDto>.Success(await GenerateAuthDtoAsync(user, ct));
    }

    public async Task<Result<AuthDto>> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (token is null || !token.IsActive)
            return Result<AuthDto>.Failure("Invalid or expired refresh token.");

        var user = await _userManager.FindByIdAsync(token.UserId);
        if (user is null)
            return Result<AuthDto>.Failure("User not found.");

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        return Result<AuthDto>.Success(await GenerateAuthDtoAsync(user, ct));
    }

    public async Task<Result<bool>> RevokeAsync(string refreshToken, CancellationToken ct)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (token is null || !token.IsActive)
            return Result<bool>.Failure("Invalid or expired refresh token.");

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private async Task<AuthDto> GenerateAuthDtoAsync(AppUser user, CancellationToken ct)
    {
        var tokenResult = _jwtService.GenerateAccessToken(user.Id, user.Email!, user.Role.ToString(), user.TenantId);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            TenantId = user.TenantId,
            ExpiresAt = refreshTokenExpiry
        });
        await _context.SaveChangesAsync(ct);

        return new AuthDto(
            tokenResult.AccessToken,
            tokenResult.ExpiresAt,
            refreshToken,
            refreshTokenExpiry,
            new UserDto(Guid.Parse(user.Id), user.Email!, user.FirstName + " " + user.LastName, user.Role, DateTime.UtcNow));
    }
}