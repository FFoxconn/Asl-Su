using AslSu.Application.Orders.Dtos;
using Xunit;

namespace AslSu.Application.Tests;

public class AssignCourierResultTests
{
    [Fact]
    public void Ok_ReturnsSuccessWithoutError()
    {
        var result = AssignCourierResult.Ok;

        Assert.True(result.Success);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Fail_ReturnsFailureWithGivenError()
    {
        var result = AssignCourierResult.Fail(OrderAssignCourierError.CourierNotFound);

        Assert.False(result.Success);
        Assert.Equal(OrderAssignCourierError.CourierNotFound, result.Error);
    }
}
