using AslSu.Domain.Entities;
using Xunit;

namespace AslSu.Domain.Tests;

public class UserTests
{
    [Fact]
    public void NewUser_DefaultsToActiveWithNoRefreshToken()
    {
        var user = new User();

        Assert.True(user.IsActive);
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiresAt);
    }
}
