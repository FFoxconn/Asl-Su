namespace AslSu.TrendyolGo.Inventory;

/// <summary>Shape of a single item in a price-and-inventory batch request. Field names/casing
/// and exactly where a per-branch store identifier belongs (payload item vs. request root) are
/// a best-effort guess pending confirmation against developers.tgoapps.com — the endpoint path
/// itself (POST /integrator/product/grocery/suppliers/{sellerId}/products/price-and-inventory)
/// was given verbatim in the integration brief, unlike the product-creation endpoint.</summary>
public record TrendyolInventoryItemPayload(
    string Barcode,
    string? StoreId,
    int Quantity,
    decimal SalePrice,
    decimal ListPrice);
