namespace AslSu.Application.Couriers.Dtos;

public record CourierDto(int Id, string Name, string? Phone, bool IsActive);

public record CreateCourierRequest(string Name, string? Phone);

public record CourierStatsDto(
    int CourierId,
    string CourierName,
    int TotalOrders,
    int DeliveredOrders,
    int CancelledOrders,
    int ReturnedOrders,
    decimal TotalRevenue,
    IReadOnlyList<CourierOrderSummaryDto> RecentOrders);

public record CourierOrderSummaryDto(
    int Id,
    string OrderNumber,
    DateTime OrderDate,
    string Status,
    decimal? InvoiceAmount);