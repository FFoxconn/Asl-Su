namespace AslSu.Application.OrderSync.Dtos;

public record OrderSyncSummary(
    int TotalFetched,
    int NewCount,
    int UpdatedCount,
    int SkippedCount,
    string? Message);
