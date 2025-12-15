using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using DocuFlow.Domain.Entities;
using DocuFlow.Domain.Enums;
using DocuFlow.Infrastructure.Identity;
using DocuFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocuFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly AppDbContext _context;

    public AuthService(
        UserManager<AppUser> userManager,
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IJwtService jwtService,
        AppDbContext context)
    {
        _userManager = userManager;
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<Result<AuthDto>> LoginAsync(string email, string password, CancellationToken ct)
    {
        var appUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
        if (appUser is null)
            return Result<AuthDto>.Failure("Invalid credentials.");

        var validPassword = await _userManager.CheckPasswordAsync(appUser, password);
        if (!validPassword)
            return Result<AuthDto>.Failure("Invalid credentials.");

        var domainUser = await _context.DomainUsers
            .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim(), ct);
        if (domainUser is null)
            return Result<AuthDto>.Failure("User profile not found.");

        domainUser.RecordLogin();
        await _context.SaveChangesAsync(ct);

        return Result<AuthDto>.Success(await GenerateAuthDtoAsync(appUser.Id, domainUser, ct));
    }

    public async Task<Result<AuthDto>> RegisterAsync(string tenantSlug, string email, string password,
        string firstName, string lastName, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(tenantSlug, ct);
        if (tenant is null)
        {
            var tenantName = System.Text.RegularExpressions.Regex.Replace(tenantSlug, "-", " ");
            tenant = Tenant.Create(tenantName, tenantSlug);
            await _tenantRepository.AddAsync(tenant, ct);
            await _context.SaveChangesAsync(ct);
        }

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            return Result<AuthDto>.Failure("A user with this email already exists.");

        // Create Identity user (auth only)
        var appUser = new AppUser
        {
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(appUser, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<AuthDto>.Failure(errors);
        }

        // Create domain user (business logic)
        var domainUser = User.Create(tenant.Id, email, string.Empty, firstName, lastName, UserRole.Viewer);
        await _userRepository.AddAsync(domainUser, ct);
        await _context.SaveChangesAsync(ct);

        return Result<AuthDto>.Success(await GenerateAuthDtoAsync(appUser.Id, domainUser, ct));
    }

    public async Task<Result<AuthDto>> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (token is null || !token.IsActive)
            return Result<AuthDto>.Failure("Invalid or expired refresh token.");

        var appUser = await _userManager.FindByIdAsync(token.UserId);
        if (appUser is null)
            return Result<AuthDto>.Failure("User not found.");

        var domainUser = await _context.DomainUsers
            .FirstOrDefaultAsync(u => u.Email == appUser.Email!.ToLower().Trim(), ct);
        if (domainUser is null)
            return Result<AuthDto>.Failure("User profile not found.");

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        return Result<AuthDto>.Success(await GenerateAuthDtoAsync(appUser.Id, domainUser, ct));
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

    private async Task<AuthDto> GenerateAuthDtoAsync(string appUserId, User domainUser, CancellationToken ct)
    {
        var tokenResult = _jwtService.GenerateAccessToken(
            domainUser.Id.ToString(), domainUser.Email, domainUser.Role.ToString(), domainUser.TenantId);

        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            UserId = appUserId,
            TenantId = domainUser.TenantId,
            ExpiresAt = refreshTokenExpiry
        });
        await _context.SaveChangesAsync(ct);

        return new AuthDto(
            tokenResult.AccessToken,
            tokenResult.ExpiresAt,
            refreshToken,
            refreshTokenExpiry,
            new UserDto(domainUser.Id, domainUser.Email, domainUser.FullName, domainUser.Role, domainUser.CreatedAt));
    }
}