namespace AslSu.TrendyolGo.PackageStatus;

public record TrendyolPackageActionOutcome(bool Success, string? ErrorMessage)
{
    public static readonly TrendyolPackageActionOutcome Ok = new(true, null);

    public static TrendyolPackageActionOutcome Fail(string errorMessage) => new(false, errorMessage);
}
