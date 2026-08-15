using AslSu.Application.Catalog;
using AslSu.Application.Catalog.Dtos;
using AslSu.Domain.Entities;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.Catalog;

public class CatalogService(AslSuDbContext dbContext) : ICatalogService
{
    public async Task<IReadOnlyList<StoreDto>> GetStoresAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Stores
            .Select(s => new StoreDto(s.Id, s.Name, s.Code, s.Address, s.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<StoreDto> CreateStoreAsync(CreateStoreRequest request, CancellationToken cancellationToken = default)
    {
        var store = new Store { Name = request.Name, Code = request.Code, Address = request.Address, IsActive = true };
        dbContext.Stores.Add(store);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new StoreDto(store.Id, store.Name, store.Code, store.Address, store.IsActive);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Categories
            .Select(c => new CategoryDto(c.Id, c.Name, c.ParentCategoryId))
            .ToListAsync(cancellationToken);

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new Category { Name = request.Name, ParentCategoryId = request.ParentCategoryId };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.ParentCategoryId);
    }

    public async Task<IReadOnlyList<BrandDto>> GetBrandsAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Brands
            .Select(b => new BrandDto(b.Id, b.Name))
            .ToListAsync(cancellationToken);

    public async Task<BrandDto> CreateBrandAsync(CreateBrandRequest request, CancellationToken cancellationToken = default)
    {
        var brand = new Brand { Name = request.Name };
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new BrandDto(brand.Id, brand.Name);
    }
}
