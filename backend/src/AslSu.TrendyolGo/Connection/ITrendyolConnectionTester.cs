namespace AslSu.TrendyolGo.Connection;

public interface ITrendyolConnectionTester
{
    Task<TrendyolConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default);
}
