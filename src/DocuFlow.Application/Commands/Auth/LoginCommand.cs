using DocuFlow.Application.Common;
using DocuFlow.Application.DTOs;
using MediatR;

namespace DocuFlow.Application.Commands.Auth;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<AuthDto>>;