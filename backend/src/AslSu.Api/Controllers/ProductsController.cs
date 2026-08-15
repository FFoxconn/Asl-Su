using AslSu.Application.Products;
using AslSu.Application.Products.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AslSu.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await productService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await productService.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Product!.Id }, result.Product)
            : Problem(MapError(result.Error!.Value), statusCode: StatusCodes.Status409Conflict);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await productService.UpdateAsync(id, request, cancellationToken);
        if (result.Success)
        {
            return Ok(result.Product);
        }

        return result.Error == ProductError.NotFound
            ? NotFound()
            : Problem(MapError(result.Error!.Value), statusCode: StatusCodes.Status409Conflict);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        await productService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    private static string MapError(ProductError error) => error switch
    {
        ProductError.DuplicateSku => "Bu SKU zaten kullanılıyor.",
        ProductError.DuplicateBarcode => "Bu barkod zaten kullanılıyor.",
        ProductError.NotFound => "Ürün bulunamadı.",
        _ => "İşlem başarısız oldu.",
    };
}
