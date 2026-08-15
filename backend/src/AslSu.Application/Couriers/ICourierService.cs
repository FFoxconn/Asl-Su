using AslSu.Application.Couriers.Dtos;

namespace AslSu.Application.Couriers;

public interface ICourierService
{
    Task<IReadOnlyList<CourierDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CourierDto> CreateAsync(CreateCourierRequest request, CancellationToken cancellationToken = default);
}
