using AslSu.Application.StockPrice;
using AslSu.Application.StockPrice.Dtos;
using AslSu.Domain.Entities;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.StockPrice;

public class StockPriceService(AslSuDbContext dbContext) : IStockPriceService
{
    public async Task<IReadOnlyList<StockPriceDto>> GetByProductAsync(int productId, CancellationToken cancellationToken = default) =>
        await dbContext.StoreProductInventories
            .Where(i => i.ProductId == productId)
            .Join(dbContext.Stores, i => i.StoreId, s => s.Id, (i, s) => new StockPriceDto(
                i.ProductId, i.StoreId, s.Name, i.Quantity, i.SalePrice, i.ListPrice, i.LastSyncedAt))
            .ToListAsync(cancellationToken);

    public async Task<StockPriceDto> UpsertAsync(UpsertStockPriceRequest request, CancellationToken cancellationToken = default)
    {
        var inventory = await dbContext.StoreProductInventories
            .SingleOrDefaultAsync(i => i.ProductId == request.ProductId && i.StoreId == request.StoreId, cancellationToken);

        if (inventory is null)
        {
            inventory = new StoreProductInventory
            {
                ProductId = request.ProductId,
                StoreId = request.StoreId,
            };
            dbContext.StoreProductInventories.Add(inventory);
        }

        inventory.Quantity = request.Quantity;
        inventory.SalePrice = request.SalePrice;
        inventory.ListPrice = request.ListPrice;

        await dbContext.SaveChangesAsync(cancellationToken);

        var storeName = await dbContext.Stores
            .Where(s => s.Id == request.StoreId)
            .Select(s => s.Name)
            .SingleAsync(cancellationToken);

        return new StockPriceDto(
            inventory.ProductId, inventory.StoreId, storeName,
            inventory.Quantity, inventory.SalePrice, inventory.ListPrice, inventory.LastSyncedAt);
    }
}
