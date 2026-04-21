namespace ServiceTicket.Core.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    string Email,
    string FullName,
    DateTime ExpiresAt
);