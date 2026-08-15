using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using Xunit;

namespace AslSu.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void NewProduct_DefaultsToActiveAndNotSynced()
    {
        var product = new Product();

        Assert.True(product.IsActive);
        Assert.Equal(TgoSyncStatus.NotSynced, product.TgoSyncStatus);
        Assert.Null(product.TgoLastSyncDate);
    }
}
