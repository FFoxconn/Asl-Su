using AslSu.Application.Catalog.Dtos;

namespace AslSu.Application.Catalog;

public interface ICatalogService
{
    Task<IReadOnlyList<StoreDto>> GetStoresAsync(CancellationToken cancellationToken = default);
    Task<StoreDto> CreateStoreAsync(CreateStoreRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BrandDto>> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<BrandDto> CreateBrandAsync(CreateBrandRequest request, CancellationToken cancellationToken = default);
}
