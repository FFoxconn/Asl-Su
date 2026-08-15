namespace AslSu.Application.Couriers.Dtos;

public record CourierDto(int Id, string Name, string? Phone, bool IsActive);

public record CreateCourierRequest(string Name, string? Phone);
