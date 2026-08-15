namespace AslSu.Domain.Entities;

public class SyncLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string SupplierId { get; set; } = string.Empty;
    public int? StoreId { get; set; }
    public string? BatchRequestId { get; set; }
    public string? OrderNumber { get; set; }
    public bool Success { get; set; }

    /// <summary>Sanitized message only — never contains API keys/secrets or Authorization headers.</summary>
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
}
