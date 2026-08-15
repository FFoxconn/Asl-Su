using AslSu.Domain.Entities;
using Xunit;

namespace AslSu.Domain.Tests;

public class CourierTests
{
    [Fact]
    public void NewCourier_DefaultsToActive()
    {
        var courier = new Courier();

        Assert.True(courier.IsActive);
    }
}
