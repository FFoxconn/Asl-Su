namespace AslSu.Domain.Entities;

/// <summary>Per-store stock and price for a product. Merged into one table because Trendyol
/// Go's price-and-inventory endpoint pushes both together.</summary>
public class StoreProductInventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int StoreId { get; set; }

    public int Quantity { get; set; }
    public decimal SalePrice { get; set; }
    public decimal ListPrice { get; set; }

    public int? LastSyncedQuantity { get; set; }
    public decimal? LastSyncedSalePrice { get; set; }
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>Desired sale status for this product at this store — toggled locally, then
    /// pushed to Trendyol Go by the sale-status sync.</summary>
    public bool IsOnSale { get; set; } = true;

    /// <summary>Only meaningful when IsOnSale is false. Must be one of Trendyol Go's supported
    /// unsell reason codes — verify against developers.tgoapps.com; not enforced as a closed
    /// set here since the real values weren't given in the integration brief.</summary>
    public string? UnsaleReasonCode { get; set; }

    public bool? LastSyncedIsOnSale { get; set; }
    public DateTime? SaleStatusLastSyncedAt { get; set; }
}
