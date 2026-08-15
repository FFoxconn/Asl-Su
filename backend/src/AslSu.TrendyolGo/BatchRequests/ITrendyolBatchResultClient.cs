namespace AslSu.TrendyolGo.BatchRequests;

public interface ITrendyolBatchResultClient
{
    /// <summary>Polls the outcome of a previously submitted batch (product creation or
    /// price-and-inventory). Returns a "not configured" failure — never throws — if
    /// TrendyolGoOptions.BatchResultEndpointPath hasn't been set yet.</summary>
    Task<TrendyolBatchResultOutcome> GetBatchRequestResultAsync(
        string batchRequestId, CancellationToken cancellationToken = default);
}
