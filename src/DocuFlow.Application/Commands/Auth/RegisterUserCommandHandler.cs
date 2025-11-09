using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Commands.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthDto>>
{
    private readonly IAuthService _authService;

    public RegisterUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(request.TenantSlug, request.Email, request.Password, request.FirstName, request.LastName, cancellationToken);
    }
}