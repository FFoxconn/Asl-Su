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
    public async Task<IReadOnlyList<CourierDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var couriers = await dbContext.Couriers.OrderBy(c => c.Name).ToListAsync(cancellationToken);
        var courierIds = couriers.Select(c => c.Id).ToList();

        var orderRows = await dbContext.Orders
            .Where(o => o.CourierId != null && courierIds.Contains(o.CourierId.Value))
            .Select(o => new { o.CourierId, o.InvoiceAmount })
            .ToListAsync(cancellationToken);

        var orderStats = orderRows
            .GroupBy(o => o.CourierId!.Value)
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Revenue: g.Sum(o => o.InvoiceAmount ?? 0)));

        return couriers.Select(c =>
        {
            orderStats.TryGetValue(c.Id, out var stats);
            return new CourierDto(
                c.Id, c.Name, c.Phone, c.IsActive, c.UserId != null,
                c.Latitude, c.Longitude, c.LocationUpdatedAt,
                stats.Count, stats.Revenue);
        }).ToList();
    }

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
        return await ToDtoAsync(courier, cancellationToken);
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

    public async Task<CourierDto?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var courier = await dbContext.Couriers.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        return courier is null ? null : await ToDtoAsync(courier, cancellationToken);
    }

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

    public async Task<CourierDto?> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var courier = await dbContext.Couriers.FindAsync([id], cancellationToken);
        if (courier is null)
        {
            return null;
        }

        courier.IsActive = isActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(courier, cancellationToken);
    }

    public async Task<CourierDto?> SetLoginAsync(
        int id, string email, string password, CancellationToken cancellationToken = default)
    {
        var courier = await dbContext.Couriers.FindAsync([id], cancellationToken);
        if (courier is null)
        {
            return null;
        }

        if (courier.UserId is null)
        {
            var user = new User
            {
                Email = email,
                DisplayName = courier.Name,
                Role = UserRole.Courier,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            user.PasswordHash = passwordHasher.Hash(user, password);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
            courier.UserId = user.Id;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var user = await dbContext.Users.FindAsync([courier.UserId.Value], cancellationToken);
            if (user is not null)
            {
                user.Email = email;
                user.PasswordHash = passwordHasher.Hash(user, password);
                user.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return await ToDtoAsync(courier, cancellationToken);
    }

    private async Task<CourierDto> ToDtoAsync(Courier courier, CancellationToken cancellationToken)
    {
        var invoiceAmounts = await dbContext.Orders
            .Where(o => o.CourierId == courier.Id)
            .Select(o => o.InvoiceAmount)
            .ToListAsync(cancellationToken);

        return new CourierDto(
            courier.Id, courier.Name, courier.Phone, courier.IsActive, courier.UserId != null,
            courier.Latitude, courier.Longitude, courier.LocationUpdatedAt,
            invoiceAmounts.Count, invoiceAmounts.Sum(a => a ?? 0));
    }
}
