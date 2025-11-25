using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthDto>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
    }
}