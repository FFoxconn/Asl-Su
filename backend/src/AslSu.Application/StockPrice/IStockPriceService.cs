using AslSu.Application.StockPrice.Dtos;

namespace AslSu.Application.StockPrice;

public interface IStockPriceService
{
    Task<IReadOnlyList<StockPriceDto>> GetByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<StockPriceDto> UpsertAsync(UpsertStockPriceRequest request, CancellationToken cancellationToken = default);

    /// <summary>Sets the desired sale status locally (picked up by the sale-status sync push
    /// afterwards). Returns null if no stock/price row exists yet for this product+store.</summary>
    Task<StockPriceDto?> SetSaleStatusAsync(SetSaleStatusRequest request, CancellationToken cancellationToken = default);
}
