using AslSu.Application.Couriers;
using AslSu.Application.Couriers.Dtos;
using AslSu.Domain.Entities;
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
}
