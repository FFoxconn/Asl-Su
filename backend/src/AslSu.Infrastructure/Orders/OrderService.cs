using AslSu.Application.Orders;
using AslSu.Application.Orders.Dtos;
using AslSu.Domain.Entities;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.Orders;

public class OrderService(AslSuDbContext dbContext) : IOrderService
{
    public async Task<IReadOnlyList<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await dbContext.Orders.OrderByDescending(o => o.OrderDate).ToListAsync(cancellationToken);
        var customerIds = orders.Where(o => o.CustomerId.HasValue).Select(o => o.CustomerId!.Value).Distinct().ToList();
        var customers = await dbContext.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        return orders.Select(o => new OrderListItemDto(
            o.Id,
            o.PackageId,
            o.OrderNumber,
            o.StoreId,
            o.Status.ToString(),
            o.WorkflowStatus.ToString(),
            o.OrderDate,
            o.InvoiceAmount,
            o.CustomerId.HasValue && customers.TryGetValue(o.CustomerId.Value, out var c) ? c.Name : null)).ToList();
    }

    public async Task<OrderDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders.FindAsync([id], cancellationToken);
        if (order is null)
        {
            return null;
        }

        var customer = order.CustomerId.HasValue
            ? await dbContext.Customers.FindAsync([order.CustomerId.Value], cancellationToken)
            : null;

        var items = await dbContext.OrderItems
            .Where(i => i.OrderId == order.Id)
            .Select(i => new OrderItemDto(i.Id, i.ProductId, i.Barcode, i.Quantity, i.UnitPrice, i.IsSubstitution, i.SubstitutedForBarcode))
            .ToListAsync(cancellationToken);

        return new OrderDetailDto(
            order.Id,
            order.PackageId,
            order.OrderNumber,
            order.StoreId,
            order.Status.ToString(),
            order.WorkflowStatus.ToString(),
            order.OrderDate,
            order.InvoiceAmount,
            order.InvoiceTaxAmount,
            order.BagCount,
            order.ReceiptLink,
            customer?.Name,
            customer?.Phone,
            customer?.Address,
            items);
    }
}
