using AslSu.Application.Orders.Dtos;
using Xunit;

namespace AslSu.Application.Tests;

public class SubstituteOrderItemResultTests
{
    [Fact]
    public void Ok_ReturnsSuccessWithoutError()
    {
        var result = SubstituteOrderItemResult.Ok;

        Assert.True(result.Success);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Fail_ReturnsFailureWithGivenError()
    {
        var result = SubstituteOrderItemResult.Fail(OrderItemSubstituteError.ProductNotFound);

        Assert.False(result.Success);
        Assert.Equal(OrderItemSubstituteError.ProductNotFound, result.Error);
    }
}
