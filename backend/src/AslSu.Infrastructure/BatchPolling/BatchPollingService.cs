using System.Text.Json;
using AslSu.Application.BatchPolling;
using AslSu.Application.BatchPolling.Dtos;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using AslSu.TrendyolGo.BatchRequests;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.BatchPolling;

public class BatchPollingService(AslSuDbContext dbContext, ITrendyolBatchResultClient batchResultClient)
    : IBatchPollingService
{
    public async Task<BatchPollSummary> PollPendingBatchesAsync(CancellationToken cancellationToken = default)
    {
        var pending = await dbContext.BatchRequestLogs
            .Where(b => b.Status == BatchStatus.Pending && b.BatchRequestId != "")
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return new BatchPollSummary(0, 0, 0, 0, "Bekleyen parti işlemi yok.");
        }

        var completedCount = 0;
        var stillProcessingCount = 0;
        var failedCount = 0;

        foreach (var log in pending)
        {
            var outcome = await batchResultClient.GetBatchRequestResultAsync(log.BatchRequestId, cancellationToken);

            if (!outcome.Success)
            {
                // Couldn't poll (e.g. BatchResultEndpointPath not configured yet) — leave the
                // row Pending and note why, rather than marking it Failed outright.
                log.FailureReasonsJson = JsonSerializer.Serialize(new[] { outcome.ErrorMessage ?? "Unknown error" });
                stillProcessingCount++;
                continue;
            }

            log.SuccessCount = outcome.SuccessCount;
            log.FailureCount = outcome.FailureCount;
            log.FailureReasonsJson = outcome.FailureReasons.Count > 0
                ? JsonSerializer.Serialize(outcome.FailureReasons)
                : null;

            log.Status = outcome.Status switch
            {
                TrendyolBatchResultStatus.Completed => BatchStatus.Completed,
                TrendyolBatchResultStatus.Failed => BatchStatus.Failed,
                _ => BatchStatus.Pending,
            };

            switch (log.Status)
            {
                case BatchStatus.Completed:
                    completedCount++;
                    break;
                case BatchStatus.Failed:
                    failedCount++;
                    break;
                default:
                    stillProcessingCount++;
                    break;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new BatchPollSummary(
            pending.Count, completedCount, stillProcessingCount, failedCount,
            $"{pending.Count} parti kontrol edildi.");
    }
}
