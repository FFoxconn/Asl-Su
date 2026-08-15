using AslSu.Application.Products;
using AslSu.Application.Products.Dtos;
using AslSu.Domain.Entities;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.Products;

public class ProductService(AslSuDbContext dbContext) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await dbContext.Products.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FindAsync([id], cancellationToken);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductResult> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Products.AnyAsync(p => p.Sku == request.Sku, cancellationToken))
        {
            return ProductResult.Fail(ProductError.DuplicateSku);
        }

        if (await dbContext.Products.AnyAsync(p => p.Barcode == request.Barcode, cancellationToken))
        {
            return ProductResult.Fail(ProductError.DuplicateBarcode);
        }

        var product = new Product
        {
            Sku = request.Sku,
            Barcode = request.Barcode,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            VatRate = request.VatRate,
            ImageUrl = request.ImageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ProductResult.Ok(ToDto(product));
    }

    public async Task<ProductResult> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FindAsync([id], cancellationToken);
        if (product is null)
        {
            return ProductResult.Fail(ProductError.NotFound);
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.VatRate = request.VatRate;
        product.ImageUrl = request.ImageUrl;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ProductResult.Ok(ToDto(product));
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FindAsync([id], cancellationToken);
        if (product is null)
        {
            return false;
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ProductDto ToDto(Product p) => new(
        p.Id, p.Sku, p.Barcode, p.Name, p.Description, p.CategoryId, p.BrandId,
        p.VatRate, p.ImageUrl, p.IsActive, p.TgoSyncStatus.ToString());
}
