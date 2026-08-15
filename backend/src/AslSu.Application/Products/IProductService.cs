using AslSu.Application.Products.Dtos;

namespace AslSu.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductResult> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResult> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
