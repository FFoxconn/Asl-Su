using AslSu.Application.Couriers.Dtos;

namespace AslSu.Application.Couriers;

public interface ICourierService
{
    Task<IReadOnlyList<CourierDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CourierDto> CreateAsync(CreateCourierRequest request, CancellationToken cancellationToken = default);
    Task<CourierStatsDto?> GetStatsAsync(int courierId, CancellationToken cancellationToken = default);

    /// <summary>Resolves the Courier row linked to a logged-in Courier-role User, if any.</summary>
    Task<CourierDto?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Records the current courier's GPS position. Returns false if no Courier row is
    /// linked to this user.</summary>
    Task<bool> UpdateLocationAsync(int userId, double latitude, double longitude, CancellationToken cancellationToken = default);
}