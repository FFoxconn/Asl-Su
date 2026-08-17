namespace AslSu.Domain.Entities;

public class Courier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Optional link to the User account this courier logs in with (Role = Courier).
    /// Null for couriers who don't have app access.</summary>
    public int? UserId { get; set; }

    /// <summary>Last GPS position reported by the courier's app. Null until they've sent one.</summary>
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? LocationUpdatedAt { get; set; }
}
