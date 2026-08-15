namespace AslSu.Application.ProductSync.Dtos;

public record BatchRequestLogDto(
    int Id,
    string BatchRequestId,
    string OperationType,
    DateTime RequestedAt,
    string Status,
    int SuccessCount,
    int FailureCount,
    string? FailureReasonsJson);
