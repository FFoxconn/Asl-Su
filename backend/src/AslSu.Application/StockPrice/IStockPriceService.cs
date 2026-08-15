using AslSu.Application.StockPrice.Dtos;

namespace AslSu.Application.StockPrice;

public interface IStockPriceService
{
    Task<IReadOnlyList<StockPriceDto>> GetByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<StockPriceDto> UpsertAsync(UpsertStockPriceRequest request, CancellationToken cancellationToken = default);
}
