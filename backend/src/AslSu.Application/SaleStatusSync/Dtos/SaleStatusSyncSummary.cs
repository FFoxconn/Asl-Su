namespace AslSu.Application.SaleStatusSync.Dtos;

public record SaleStatusSyncSummary(
    int TotalItems,
    int BatchCount,
    int SubmittedCount,
    int FailedCount,
    IReadOnlyList<string> BatchRequestIds,
    string? Message);
