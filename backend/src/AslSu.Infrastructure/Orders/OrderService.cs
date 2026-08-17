using AslSu.Application.Orders;
using AslSu.Application.Orders.Dtos;
using AslSu.Domain.Entities;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.Orders;

public class OrderService(AslSuDbContext dbContext) : IOrderService
{
    public async Task<IReadOnlyList<OrderListItemDto>> GetAllAsync(
        int? courierId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders.AsQueryable();
        if (courierId.HasValue)
        {
            query = query.Where(o => o.CourierId == courierId.Value);
        }

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync(cancellationToken);
        var customerIds = orders.Where(o => o.CustomerId.HasValue).Select(o => o.CustomerId!.Value).Distinct().ToList();
        var customers = await dbContext.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);
        var courierIds = orders.Where(o => o.CourierId.HasValue).Select(o => o.CourierId!.Value).Distinct().ToList();
        var couriers = await dbContext.Couriers
            .Where(c => courierIds.Contains(c.Id))
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
            o.CustomerId.HasValue && customers.TryGetValue(o.CustomerId.Value, out var c) ? c.Name : null,
            o.CourierId,
            o.CourierId.HasValue && couriers.TryGetValue(o.CourierId.Value, out var cr) ? cr.Name : null,
            o.UpdatedAt)).ToList();
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
        var courier = order.CourierId.HasValue
            ? await dbContext.Couriers.FindAsync([order.CourierId.Value], cancellationToken)
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
            order.CourierId,
            courier?.Name,
            items);
    }

    public async Task<AssignCourierResult> AssignCourierAsync(
        int orderId, int courierId, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders.FindAsync([orderId], cancellationToken);
        if (order is null)
        {
            return AssignCourierResult.Fail(OrderAssignCourierError.OrderNotFound);
        }

        var courier = await dbContext.Couriers.FindAsync([courierId], cancellationToken);
        if (courier is null)
        {
            return AssignCourierResult.Fail(OrderAssignCourierError.CourierNotFound);
        }

        order.CourierId = courierId;
        order.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return AssignCourierResult.Ok;
    }

    public async Task<SubstituteOrderItemResult> SubstituteOrderItemAsync(
        int orderId, int itemId, int newProductId, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders.FindAsync([orderId], cancellationToken);
        if (order is null)
        {
            return SubstituteOrderItemResult.Fail(OrderItemSubstituteError.OrderNotFound);
        }

        var item = await dbContext.OrderItems.FirstOrDefaultAsync(
            i => i.Id == itemId && i.OrderId == orderId, cancellationToken);
        if (item is null)
        {
            return SubstituteOrderItemResult.Fail(OrderItemSubstituteError.ItemNotFound);
        }

        var product = await dbContext.Products.FindAsync([newProductId], cancellationToken);
        if (product is null)
        {
            return SubstituteOrderItemResult.Fail(OrderItemSubstituteError.ProductNotFound);
        }

        item.SubstitutedForBarcode = item.Barcode;
        item.ProductId = product.Id;
        item.Barcode = product.Barcode;
        item.IsSubstitution = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return SubstituteOrderItemResult.Ok;
    }
}
