namespace AslSu.Application.Orders.Dtos;

public record OrderItemDto(
    int Id,
    int? ProductId,
    string Barcode,
    int Quantity,
    decimal UnitPrice,
    bool IsSubstitution,
    string? SubstitutedForBarcode);

public record OrderListItemDto(
    int Id,
    string PackageId,
    string OrderNumber,
    int StoreId,
    string Status,
    string WorkflowStatus,
    DateTime OrderDate,
    decimal? InvoiceAmount,
    string? CustomerName,
    int? CourierId,
    string? CourierName);

public record OrderDetailDto(
    int Id,
    string PackageId,
    string OrderNumber,
    int StoreId,
    string Status,
    string WorkflowStatus,
    DateTime OrderDate,
    decimal? InvoiceAmount,
    decimal? InvoiceTaxAmount,
    int? BagCount,
    string? ReceiptLink,
    string? CustomerName,
    string? CustomerPhone,
    string? CustomerAddress,
    int? CourierId,
    string? CourierName,
    IReadOnlyList<OrderItemDto> Items);

public record AssignCourierRequest(int CourierId);

public record SubstituteOrderItemRequest(int ProductId);

public enum OrderAssignCourierError
{
    OrderNotFound,
    CourierNotFound,
}

public record AssignCourierResult(bool Success, OrderAssignCourierError? Error)
{
    public static readonly AssignCourierResult Ok = new(true, null);

    public static AssignCourierResult Fail(OrderAssignCourierError error) => new(false, error);
}

public enum OrderItemSubstituteError
{
    OrderNotFound,
    ItemNotFound,
    ProductNotFound,
}

public record SubstituteOrderItemResult(bool Success, OrderItemSubstituteError? Error)
{
    public static readonly SubstituteOrderItemResult Ok = new(true, null);

    public static SubstituteOrderItemResult Fail(OrderItemSubstituteError error) => new(false, error);
}
