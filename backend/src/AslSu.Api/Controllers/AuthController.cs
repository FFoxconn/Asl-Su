using AslSu.Application.Auth;
using AslSu.Application.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AslSu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.Success
            ? Ok(result.Response)
            : Problem(MapError(result.Error!.Value), statusCode: StatusCodes.Status401Unauthorized);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, cancellationToken);
        return result.Success
            ? Ok(result.Response)
            : Problem(MapError(result.Error!.Value), statusCode: StatusCodes.Status401Unauthorized);
    }

    private static string MapError(AuthError error) => error switch
    {
        AuthError.InvalidCredentials => "Email or password is incorrect.",
        AuthError.AccountInactive => "This account is inactive.",
        AuthError.InvalidRefreshToken => "Refresh token is invalid.",
        AuthError.RefreshTokenExpired => "Refresh token has expired. Please log in again.",
        _ => "Authentication failed.",
    };

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
        name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
        role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
    });
}
