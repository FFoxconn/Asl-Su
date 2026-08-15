namespace AslSu.Application.StockPrice.Dtos;

public record StockPriceDto(
    int ProductId,
    int StoreId,
    string StoreName,
    int Quantity,
    decimal SalePrice,
    decimal ListPrice,
    DateTime? LastSyncedAt);

public record UpsertStockPriceRequest(
    int ProductId,
    int StoreId,
    int Quantity,
    decimal SalePrice,
    decimal ListPrice);
