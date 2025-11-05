namespace DocuFlow.Application.DTOs;

public record AuthDto(
    string AccessToken,
    DateTime ExpiresAt,
    UserDto User
);