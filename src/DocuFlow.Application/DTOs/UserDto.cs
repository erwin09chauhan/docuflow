using DocuFlow.Domain.Enums;

namespace DocuFlow.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    DateTime CreatedAt
);