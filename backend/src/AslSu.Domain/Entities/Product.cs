using AslSu.Domain.Enums;

namespace AslSu.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal VatRate { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public string? TgoProductId { get; set; }
    public string? TgoBarcode { get; set; }
    public TgoSyncStatus TgoSyncStatus { get; set; } = TgoSyncStatus.NotSynced;
    public DateTime? TgoLastSyncDate { get; set; }
    public string? TgoLastError { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
