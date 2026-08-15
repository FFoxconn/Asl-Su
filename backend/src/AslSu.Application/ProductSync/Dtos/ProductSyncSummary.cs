namespace AslSu.Application.ProductSync.Dtos;

public record ProductSyncSummary(
    int TotalProducts,
    int BatchCount,
    int SubmittedCount,
    int FailedCount,
    IReadOnlyList<string> BatchRequestIds,
    string? Message);
