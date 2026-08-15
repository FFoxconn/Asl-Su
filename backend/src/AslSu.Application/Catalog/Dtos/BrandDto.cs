namespace AslSu.Application.Catalog.Dtos;

public record BrandDto(int Id, string Name);

public record CreateBrandRequest(string Name);
