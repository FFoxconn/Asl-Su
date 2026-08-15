namespace AslSu.Application.Auth.Dtos;

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    string DisplayName,
    string Role);
