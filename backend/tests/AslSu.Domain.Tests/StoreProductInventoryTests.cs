using AslSu.Domain.Entities;
using Xunit;

namespace AslSu.Domain.Tests;

public class StoreProductInventoryTests
{
    [Fact]
    public void NewInventoryRow_DefaultsToOnSaleAndNeverSynced()
    {
        var inventory = new StoreProductInventory();

        Assert.True(inventory.IsOnSale);
        Assert.Null(inventory.LastSyncedQuantity);
        Assert.Null(inventory.LastSyncedSalePrice);
        Assert.Null(inventory.LastSyncedIsOnSale);
    }
}
