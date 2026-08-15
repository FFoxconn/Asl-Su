using AslSu.Domain.Enums;

namespace AslSu.Domain.Entities;

public class BatchRequestLog
{
    public int Id { get; set; }
    public string BatchRequestId { get; set; } = string.Empty;
    public BatchOperationType OperationType { get; set; }
    public DateTime RequestedAt { get; set; }
    public BatchStatus Status { get; set; } = BatchStatus.Pending;
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public string? FailureReasonsJson { get; set; }
}
