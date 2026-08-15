using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using Xunit;

namespace AslSu.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void NewOrder_DefaultsToNewWorkflowStatus()
    {
        var order = new Order();

        Assert.Equal(WorkflowStatus.New, order.WorkflowStatus);
    }

    [Fact]
    public void NewOrder_HasNoCourierAssignedByDefault()
    {
        var order = new Order();

        Assert.Null(order.CourierId);
    }

    [Fact]
    public void NewOrderItem_IsNotASubstitutionByDefault()
    {
        var item = new OrderItem();

        Assert.False(item.IsSubstitution);
        Assert.Null(item.SubstitutedForBarcode);
    }
}
