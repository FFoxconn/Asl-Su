namespace AslSu.Application.Catalog.Dtos;

public record StoreDto(int Id, string Name, string Code, string? Address, bool IsActive);

public record CreateStoreRequest(string Name, string Code, string? Address);
