namespace AslSu.TrendyolGo.BatchRequests;

public static class BatchSplitter
{
    /// <summary>Splits a list into chunks of at most <paramref name="maxBatchSize"/> items.
    /// Default of 1000 matches the user's stated understanding of the Trendyol Go batch limit
    /// — TODO: verify the actual limit against developers.tgoapps.com before relying on it.</summary>
    public static IReadOnlyList<IReadOnlyList<T>> Split<T>(IReadOnlyList<T> items, int maxBatchSize = 1000)
    {
        if (maxBatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBatchSize), "Batch size must be positive.");
        }

        if (items.Count == 0)
        {
            return [];
        }

        var batches = new List<IReadOnlyList<T>>();
        for (var offset = 0; offset < items.Count; offset += maxBatchSize)
        {
            var size = Math.Min(maxBatchSize, items.Count - offset);
            batches.Add(items.Skip(offset).Take(size).ToList());
        }

        return batches;
    }
}
