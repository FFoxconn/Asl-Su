using AslSu.Application.Products.Dtos;

namespace AslSu.Application.Products;

public enum ProductError
{
    DuplicateSku,
    DuplicateBarcode,
    NotFound,
}

public record ProductResult(bool Success, ProductDto? Product, ProductError? Error)
{
    public static ProductResult Ok(ProductDto product) => new(true, product, null);
    public static ProductResult Fail(ProductError error) => new(false, null, error);
}
