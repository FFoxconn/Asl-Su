using AslSu.TrendyolGo.BatchRequests;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class BatchSplitterTests
{
    [Fact]
    public void Split_WithEmptyList_ReturnsNoBatches()
    {
        var result = BatchSplitter.Split<int>([]);

        Assert.Empty(result);
    }

    [Fact]
    public void Split_WithFewerItemsThanMaxBatchSize_ReturnsOneBatch()
    {
        var items = Enumerable.Range(1, 5).ToList();

        var result = BatchSplitter.Split(items, maxBatchSize: 1000);

        Assert.Single(result);
        Assert.Equal(5, result[0].Count);
    }

    [Fact]
    public void Split_WithExactlyMaxBatchSize_ReturnsOneBatch()
    {
        var items = Enumerable.Range(1, 1000).ToList();

        var result = BatchSplitter.Split(items, maxBatchSize: 1000);

        Assert.Single(result);
        Assert.Equal(1000, result[0].Count);
    }

    [Fact]
    public void Split_WithOneMoreThanMaxBatchSize_ReturnsTwoBatches()
    {
        var items = Enumerable.Range(1, 1001).ToList();

        var result = BatchSplitter.Split(items, maxBatchSize: 1000);

        Assert.Equal(2, result.Count);
        Assert.Equal(1000, result[0].Count);
        Assert.Single(result[1]);
    }

    [Fact]
    public void Split_PreservesItemOrderAcrossBatches()
    {
        var items = Enumerable.Range(1, 2500).ToList();

        var result = BatchSplitter.Split(items, maxBatchSize: 1000);

        Assert.Equal(3, result.Count);
        var flattened = result.SelectMany(b => b).ToList();
        Assert.Equal(items, flattened);
    }

    [Fact]
    public void Split_WithNonPositiveMaxBatchSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BatchSplitter.Split([1, 2, 3], maxBatchSize: 0));
    }
}
