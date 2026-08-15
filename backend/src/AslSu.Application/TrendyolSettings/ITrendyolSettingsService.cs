using AslSu.Application.TrendyolSettings.Dtos;

namespace AslSu.Application.TrendyolSettings;

public interface ITrendyolSettingsService
{
    /// <summary>Read-only, masked view of the configured credentials. There is no
    /// corresponding write endpoint — credentials are set only via user-secrets/environment
    /// variables, never through the API or stored in the database.</summary>
    TrendyolSettingsDto GetSettings();

    Task<TestConnectionResponse> TestConnectionAsync(CancellationToken cancellationToken = default);
}
