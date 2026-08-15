using AslSu.Application.OrderWorkflow.Dtos;
using Xunit;

namespace AslSu.Application.Tests;

public class OrderWorkflowActionResultTests
{
    [Fact]
    public void Ok_ReturnsSuccessWithoutError()
    {
        var result = OrderWorkflowActionResult.Ok("Kabul Edildi", trendyolNotified: true, trendyolMessage: "Bildirildi.");

        Assert.True(result.Success);
        Assert.Equal("Kabul Edildi", result.WorkflowStatus);
        Assert.True(result.TrendyolNotified);
        Assert.Equal("Bildirildi.", result.TrendyolMessage);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Fail_ReturnsFailureWithErrorAndNoStatus()
    {
        var result = OrderWorkflowActionResult.Fail(OrderWorkflowError.InvalidTransition);

        Assert.False(result.Success);
        Assert.Null(result.WorkflowStatus);
        Assert.False(result.TrendyolNotified);
        Assert.Null(result.TrendyolMessage);
        Assert.Equal(OrderWorkflowError.InvalidTransition, result.Error);
    }
}
