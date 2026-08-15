namespace AslSu.Application.Products.Dtos;

public record ProductDto(
    int Id,
    string Sku,
    string Barcode,
    string Name,
    string? Description,
    int? CategoryId,
    int? BrandId,
    decimal VatRate,
    string? ImageUrl,
    bool IsActive,
    string TgoSyncStatus);

public record CreateProductRequest(
    string Sku,
    string Barcode,
    string Name,
    string? Description,
    int? CategoryId,
    int? BrandId,
    decimal VatRate,
    string? ImageUrl);

public record UpdateProductRequest(
    string Name,
    string? Description,
    int? CategoryId,
    int? BrandId,
    decimal VatRate,
    string? ImageUrl,
    bool IsActive);
