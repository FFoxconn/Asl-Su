namespace AslSu.TrendyolGo.Products;

public interface ITrendyolProductClient
{
    /// <summary>Submits one batch (already split to the configured max batch size) of
    /// products for creation. Returns a failure result — never throws for a "not configured"
    /// state — if TrendyolGoOptions.ProductsEndpointPath hasn't been set yet.</summary>
    Task<TrendyolBatchSubmitResult> CreateProductsBatchAsync(
        IReadOnlyList<TrendyolProductPayload> products, CancellationToken cancellationToken = default);
}
