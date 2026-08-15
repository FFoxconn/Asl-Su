using System.Diagnostics;
using AslSu.Application.TrendyolSettings;
using AslSu.Application.TrendyolSettings.Dtos;
using AslSu.Domain.Entities;
using AslSu.Infrastructure.Persistence;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Connection;
using Microsoft.Extensions.Options;

namespace AslSu.Infrastructure.TrendyolSettings;

public class TrendyolSettingsService(
    IOptions<TrendyolGoOptions> options,
    ITrendyolConnectionTester connectionTester,
    AslSuDbContext dbContext) : ITrendyolSettingsService
{
    public TrendyolSettingsDto GetSettings()
    {
        var opts = options.Value;
        return new TrendyolSettingsDto(opts.SupplierId, Mask(opts.ApiKey), opts.IsConfigured);
    }

    public async Task<TestConnectionResponse> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await connectionTester.TestConnectionAsync(cancellationToken);
        stopwatch.Stop();

        var success = result.Status == TrendyolConnectionStatus.Success;

        dbContext.SyncLogs.Add(new SyncLog
        {
            Timestamp = DateTime.UtcNow,
            OperationType = "TestConnection",
            Endpoint = "/integrator/order/grocery/suppliers/{supplierId}/packages",
            HttpMethod = "GET",
            StatusCode = result.HttpStatusCode ?? 0,
            SupplierId = options.Value.SupplierId,
            Success = success,
            ErrorMessage = success ? null : result.Message,
            DurationMs = stopwatch.ElapsedMilliseconds,
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TestConnectionResponse(success, result.Message);
    }

    private static string Mask(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return string.Empty;
        }

        if (apiKey.Length <= 4)
        {
            return new string('*', apiKey.Length);
        }

        return $"{apiKey[..2]}{new string('*', apiKey.Length - 4)}{apiKey[^2..]}";
    }
}
