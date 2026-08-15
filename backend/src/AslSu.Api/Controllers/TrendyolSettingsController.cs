using AslSu.Application.TrendyolSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AslSu.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/trendyol-settings")]
public class TrendyolSettingsController(ITrendyolSettingsService settingsService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetSettings() => Ok(settingsService.GetSettings());

    [HttpPost("test-connection")]
    public async Task<IActionResult> TestConnection(CancellationToken cancellationToken) =>
        Ok(await settingsService.TestConnectionAsync(cancellationToken));
}
