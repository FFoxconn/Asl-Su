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
    string? CustomerName);

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
    IReadOnlyList<OrderItemDto> Items);
