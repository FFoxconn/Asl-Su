namespace AslSu.TrendyolGo.BatchRequests;

public enum TrendyolBatchResultStatus
{
    Completed,
    Processing,
    Failed,
    Unknown,
}

public record TrendyolBatchResultOutcome(
    bool Success,
    TrendyolBatchResultStatus Status,
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<string> FailureReasons,
    string? ErrorMessage)
{
    public static TrendyolBatchResultOutcome Fail(string errorMessage) =>
        new(false, TrendyolBatchResultStatus.Unknown, 0, 0, [], errorMessage);
}
