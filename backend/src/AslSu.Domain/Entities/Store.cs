namespace AslSu.Domain.Entities;

public class Store
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Trendyol Go's identifier for this store/warehouse, if the branch-level
    /// price-and-inventory payload needs one — confirm against developers.tgoapps.com.</summary>
    public string? TgoStoreId { get; set; }
}
