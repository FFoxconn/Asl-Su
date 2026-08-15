using AslSu.Application.BatchPolling.Dtos;

namespace AslSu.Application.BatchPolling;

public interface IBatchPollingService
{
    /// <summary>Polls Trendyol Go for the outcome of every BatchRequestLog row still Pending
    /// (product creation or price-and-inventory) and updates its Status/SuccessCount/
    /// FailureCount/FailureReasonsJson accordingly.</summary>
    Task<BatchPollSummary> PollPendingBatchesAsync(CancellationToken cancellationToken = default);
}
