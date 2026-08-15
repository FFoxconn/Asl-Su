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
}
