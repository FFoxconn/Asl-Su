using AslSu.Domain.Enums;

namespace AslSu.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    /// <summary>Trendyol Go's package id — the upsert/duplicate-prevention key.</summary>
    public string PackageId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;

    public int StoreId { get; set; }
    public int? CustomerId { get; set; }
    public int? CourierId { get; set; }

    public OrderStatus Status { get; set; }
    public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.New;

    public DateTime OrderDate { get; set; }
    public decimal? InvoiceAmount { get; set; }
    public decimal? InvoiceTaxAmount { get; set; }
    public int? BagCount { get; set; }
    public string? ReceiptLink { get; set; }
    public string? RawPayloadJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
