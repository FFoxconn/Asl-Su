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

    /// <summary>Activates/deactivates a courier. Returns null if the courier doesn't exist.</summary>
    Task<CourierDto?> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>Grants login access (creating a Courier-role User the first time) or resets
    /// the password on the courier's existing one. Returns null if the courier doesn't
    /// exist, or fails with a duplicate-email error via the thrown exception from SaveChanges
    /// if another user already has that email.</summary>
    Task<CourierDto?> SetLoginAsync(int id, string email, string password, CancellationToken cancellationToken = default);
}