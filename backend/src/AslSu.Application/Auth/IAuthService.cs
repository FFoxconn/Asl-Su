using AslSu.Application.Auth.Dtos;

namespace AslSu.Application.Auth;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);
}
