namespace AslSu.TrendyolGo.Orders;

/// <summary>One line item inside a package, best-effort mapped — TODO: confirm the exact
/// field names against developers.tgoapps.com.</summary>
public record TrendyolPackageLineDto(
    string Barcode,
    int Quantity,
    decimal UnitPrice,
    bool IsSubstitution,
    string? SubstitutedForBarcode);

/// <summary>One package (order) as returned by the packages GET endpoint, best-effort mapped
/// from commonly-used marketplace field names — TODO: confirm the exact response schema
/// against developers.tgoapps.com. <see cref="RawJson"/> always preserves the original
/// element exactly as received, regardless of how well the fields above parsed, so nothing
/// is lost if this guess turns out to be wrong.</summary>
public record TrendyolPackageDto(
    string PackageId,
    string OrderNumber,
    string? Status,
    DateTime? OrderDate,
    string? CustomerName,
    string? CustomerPhone,
    string? CustomerAddress,
    decimal? InvoiceAmount,
    decimal? InvoiceTaxAmount,
    int? BagCount,
    string? ReceiptLink,
    string? StoreTgoId,
    IReadOnlyList<TrendyolPackageLineDto> Lines,
    string RawJson);

public record TrendyolPackagesOutcome(bool Success, IReadOnlyList<TrendyolPackageDto> Packages, string? ErrorMessage)
{
    public static TrendyolPackagesOutcome Fail(string errorMessage) => new(false, [], errorMessage);
}
