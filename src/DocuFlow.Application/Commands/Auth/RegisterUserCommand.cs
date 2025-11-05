using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Commands.Auth;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string TenantSlug
) : IRequest<Result<AuthDto>>;