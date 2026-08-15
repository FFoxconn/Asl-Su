namespace AslSu.Domain.Enums;

/// <summary>Mirrors the status vocabulary used by Trendyol Go for a package/order.</summary>
public enum OrderStatus
{
    Created = 0,
    Picking = 1,
    Invoiced = 2,
    Shipped = 3,
    Cancelled = 4,
    Delivered = 5,
    Returned = 6,
    UnPacked = 7,
    UnSupplied = 8,
}
