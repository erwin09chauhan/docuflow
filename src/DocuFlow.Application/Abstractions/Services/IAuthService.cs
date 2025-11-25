using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;

namespace DocuFlow.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Result<AuthDto>> LoginAsync(string email, string password, CancellationToken ct);
    Task<Result<AuthDto>> RegisterAsync(string tenantSlug, string email, string password, string firstName, string lastName, CancellationToken ct);
    Task<Result<AuthDto>> RefreshAsync(string refreshToken, CancellationToken ct);
    Task<Result<bool>> RevokeAsync(string refreshToken, CancellationToken ct);
}