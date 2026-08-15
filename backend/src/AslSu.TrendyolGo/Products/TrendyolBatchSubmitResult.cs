namespace AslSu.TrendyolGo.Products;

public record TrendyolBatchSubmitResult(bool Success, string? BatchRequestId, string? ErrorMessage)
{
    public static TrendyolBatchSubmitResult Ok(string batchRequestId) => new(true, batchRequestId, null);
    public static TrendyolBatchSubmitResult Fail(string errorMessage) => new(false, null, errorMessage);
}
