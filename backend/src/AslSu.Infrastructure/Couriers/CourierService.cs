using AslSu.Application.Abstractions;
using AslSu.Application.Couriers;
using AslSu.Application.Couriers.Dtos;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.Couriers;

public class CourierService(AslSuDbContext dbContext, IPasswordHasher passwordHasher) : ICourierService
{
    public async Task<IReadOnlyList<CourierDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Couriers
            .OrderBy(c => c.Name)
            .Select(c => new CourierDto(
                c.Id, c.Name, c.Phone, c.IsActive, c.UserId != null, c.Latitude, c.Longitude, c.LocationUpdatedAt))
            .ToListAsync(cancellationToken);

    public async Task<CourierDto> CreateAsync(CreateCourierRequest request, CancellationToken cancellationToken = default)
    {
        var courier = new Courier { Name = request.Name, Phone = request.Phone, IsActive = true };

        if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.Password))
        {
            var user = new User
            {
                Email = request.Email,
                DisplayName = request.Name,
                Role = UserRole.Courier,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            user.PasswordHash = passwordHasher.Hash(user, request.Password);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
            courier.UserId = user.Id;
        }

        dbContext.Couriers.Add(courier);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CourierDto(courier.Id, courier.Name, courier.Phone, courier.IsActive, courier.UserId != null, null, null, null);
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

    public async Task<CourierDto?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        await dbContext.Couriers
            .Where(c => c.UserId == userId)
            .Select(c => new CourierDto(
                c.Id, c.Name, c.Phone, c.IsActive, true, c.Latitude, c.Longitude, c.LocationUpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> UpdateLocationAsync(
        int userId, double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var courier = await dbContext.Couriers.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        if (courier is null)
        {
            return false;
        }

        courier.Latitude = latitude;
        courier.Longitude = longitude;
        courier.LocationUpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
