using AslSu.Application.Abstractions;
using AslSu.Application.Auth;
using AslSu.Application.Auth.Dtos;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.Auth;

public class AuthService(AslSuDbContext dbContext, ITokenService tokenService, IPasswordHasher passwordHasher)
    : IAuthService
{
    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.Verify(user, user.PasswordHash, request.Password))
        {
            return AuthResult.Fail(AuthError.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return AuthResult.Fail(AuthError.AccountInactive);
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user is null || user.RefreshTokenExpiresAt is null)
        {
            return AuthResult.Fail(AuthError.InvalidRefreshToken);
        }

        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            return AuthResult.Fail(AuthError.RefreshTokenExpired);
        }

        if (!user.IsActive)
        {
            return AuthResult.Fail(AuthError.AccountInactive);
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    private async Task<AuthResult> IssueTokensAsync(Domain.Entities.User user, CancellationToken cancellationToken)
    {
        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = tokenService.GetRefreshTokenExpiry();
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            refreshToken,
            user.DisplayName,
            user.Role.ToString());

        return AuthResult.Ok(response);
    }
}
