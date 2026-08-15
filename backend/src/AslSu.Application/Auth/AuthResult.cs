using AslSu.Application.Auth.Dtos;

namespace AslSu.Application.Auth;

public enum AuthError
{
    InvalidCredentials,
    AccountInactive,
    InvalidRefreshToken,
    RefreshTokenExpired,
}

public record AuthResult(bool Success, AuthResponse? Response, AuthError? Error)
{
    public static AuthResult Ok(AuthResponse response) => new(true, response, null);
    public static AuthResult Fail(AuthError error) => new(false, null, error);
}
