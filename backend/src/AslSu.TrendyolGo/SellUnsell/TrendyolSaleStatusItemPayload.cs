namespace AslSu.TrendyolGo.SellUnsell;

/// <summary>Shape of a single item in a sell/unsell batch request. Field names/casing are a
/// best-effort guess pending schema confirmation against developers.tgoapps.com — the
/// endpoint path itself is unconfirmed too (see TrendyolGoOptions.SellUnsellEndpointPath),
/// unlike price-and-inventory and packages-GET which were given verbatim in the integration
/// brief. ReasonCode is only meaningful when IsOnSale is false and must be one of Trendyol
/// Go's supported unsell reason codes — not validated as a closed set here since those
/// values were never given either.</summary>
public record TrendyolSaleStatusItemPayload(
    string Barcode,
    string? StoreId,
    bool IsOnSale,
    string? ReasonCode);
