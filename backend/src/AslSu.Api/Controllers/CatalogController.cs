using AslSu.Application.Catalog;
using AslSu.Application.Catalog.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AslSu.Api.Controllers;

[ApiController]
[Authorize]
public class CatalogController(ICatalogService catalogService) : ControllerBase
{
    [HttpGet("/api/stores")]
    public async Task<IActionResult> GetStores(CancellationToken cancellationToken) =>
        Ok(await catalogService.GetStoresAsync(cancellationToken));

    [HttpPost("/api/stores")]
    public async Task<IActionResult> CreateStore(CreateStoreRequest request, CancellationToken cancellationToken) =>
        Ok(await catalogService.CreateStoreAsync(request, cancellationToken));

    [HttpGet("/api/categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken) =>
        Ok(await catalogService.GetCategoriesAsync(cancellationToken));

    [HttpPost("/api/categories")]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest request, CancellationToken cancellationToken) =>
        Ok(await catalogService.CreateCategoryAsync(request, cancellationToken));

    [HttpGet("/api/brands")]
    public async Task<IActionResult> GetBrands(CancellationToken cancellationToken) =>
        Ok(await catalogService.GetBrandsAsync(cancellationToken));

    [HttpPost("/api/brands")]
    public async Task<IActionResult> CreateBrand(CreateBrandRequest request, CancellationToken cancellationToken) =>
        Ok(await catalogService.CreateBrandAsync(request, cancellationToken));
}
