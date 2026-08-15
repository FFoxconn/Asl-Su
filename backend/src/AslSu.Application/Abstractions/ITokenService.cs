using AslSu.Domain.Entities;

namespace AslSu.Application.Abstractions;

public record AccessToken(string Value, DateTime ExpiresAtUtc);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
    string CreateRefreshToken();
    DateTime GetRefreshTokenExpiry();
}
