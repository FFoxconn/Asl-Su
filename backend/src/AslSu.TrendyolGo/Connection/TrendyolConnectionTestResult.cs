namespace AslSu.TrendyolGo.Connection;

public record TrendyolConnectionTestResult(TrendyolConnectionStatus Status, string Message, int? HttpStatusCode);
