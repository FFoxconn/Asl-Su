namespace AslSu.Application.BatchPolling.Dtos;

public record BatchPollSummary(
    int PolledCount,
    int CompletedCount,
    int StillProcessingCount,
    int FailedCount,
    string? Message);
