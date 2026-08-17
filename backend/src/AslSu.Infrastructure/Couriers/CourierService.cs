using AslSu.Application.Couriers;
using AslSu.Application.Couriers.Dtos;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.Couriers;

public class CourierService(AslSuDbContext dbContext) : ICourierService
{
    public async Task<IReadOnlyList<CourierDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Couriers
            .OrderBy(c => c.Name)
            .Select(c => new CourierDto(c.Id, c.Name, c.Phone, c.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<CourierDto> CreateAsync(CreateCourierRequest request, CancellationToken cancellationToken = default)
    {
        var courier = new Courier { Name = request.Name, Phone = request.Phone, IsActive = true };
        dbContext.Couriers.Add(courier);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CourierDto(courier.Id, courier.Name, courier.Phone, courier.IsActive);
    }

    public async Task<CourierStatsDto?> GetStatsAsync(int courierId, CancellationToken cancellationToken = default)
    {
        var courier = await dbContext.Couriers.FindAsync([courierId], cancellationToken);
        if (courier is null)
        {
            return null;
        }

        var orders = await dbContext.Orders
            .Where(o => o.CourierId == courierId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);

        var recentOrders = orders
            .Take(20)
            .Select(o => new CourierOrderSummaryDto(o.Id, o.OrderNumber, o.OrderDate, o.Status.ToString(), o.InvoiceAmount))
            .ToList();

        return new CourierStatsDto(
            courier.Id,
            courier.Name,
            orders.Count,
            orders.Count(o => o.Status == OrderStatus.Delivered),
            orders.Count(o => o.Status == OrderStatus.Cancelled),
            orders.Count(o => o.Status == OrderStatus.Returned),
            orders.Sum(o => o.InvoiceAmount ?? 0),
            recentOrders);
    }
}