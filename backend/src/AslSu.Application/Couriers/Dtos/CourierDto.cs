namespace AslSu.Application.Couriers.Dtos;

public record CourierDto(
    int Id,
    string Name,
    string? Phone,
    bool IsActive,
    bool HasLogin,
    double? Latitude,
    double? Longitude,
    DateTime? LocationUpdatedAt,
    int TotalOrders,
    decimal TotalRevenue);

/// <summary>Email/Password are optional — set both together to also create a Courier-role
/// login account for this courier (skip both to keep it a login-less reference row).</summary>
public record CreateCourierRequest(string Name, string? Phone, string? Email = null, string? Password = null);

public record UpdateCourierLocationRequest(double Latitude, double Longitude);

public record SetCourierActiveRequest(bool IsActive);

/// <summary>Grants a courier login access if it doesn't have one yet (creates a linked
/// Courier-role User), or resets the password on the existing one.</summary>
public record SetCourierLoginRequest(string Email, string Password);

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
