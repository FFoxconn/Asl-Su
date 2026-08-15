namespace AslSu.Application.StockPriceSync.Dtos;

public record StockPriceSyncSummary(
    int TotalItems,
    int BatchCount,
    int SubmittedCount,
    int FailedCount,
    IReadOnlyList<string> BatchRequestIds,
    string? Message);
