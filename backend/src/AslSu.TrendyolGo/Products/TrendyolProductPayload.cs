namespace AslSu.TrendyolGo.Products;

/// <summary>Shape of a single product in a createProducts batch request. Field names/casing
/// are a reasonable best guess from common Trendyol integration conventions — TODO: confirm
/// the exact request schema against developers.tgoapps.com before relying on this for a real
/// submission (the endpoint path itself is unconfigured for the same reason; see
/// TrendyolGoOptions.ProductsEndpointPath).</summary>
public record TrendyolProductPayload(
    string Barcode,
    string Title,
    string? Brand,
    string? Category,
    decimal VatRate,
    string StockCode,
    string? Description,
    string? ImageUrl);
