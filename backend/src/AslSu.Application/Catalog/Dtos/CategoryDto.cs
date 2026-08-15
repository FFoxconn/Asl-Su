namespace AslSu.Application.Catalog.Dtos;

public record CategoryDto(int Id, string Name, int? ParentCategoryId);

public record CreateCategoryRequest(string Name, int? ParentCategoryId);
