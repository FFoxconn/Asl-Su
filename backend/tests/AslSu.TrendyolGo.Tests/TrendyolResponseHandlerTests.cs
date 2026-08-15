using System.Net;
using AslSu.TrendyolGo.Exceptions;
using AslSu.TrendyolGo.Http;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class TrendyolResponseHandlerTests
{
    [Fact]
    public async Task EnsureSuccessAsync_WithSuccessStatus_DoesNotThrow()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        await TrendyolResponseHandler.EnsureSuccessAsync(response);
    }

    [Fact]
    public async Task EnsureSuccessAsync_With401_ThrowsTrendyolAuthException()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        var ex = await Assert.ThrowsAsync<TrendyolAuthException>(() => TrendyolResponseHandler.EnsureSuccessAsync(response));
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public async Task EnsureSuccessAsync_With403_ThrowsTrendyolAuthException()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
        var ex = await Assert.ThrowsAsync<TrendyolAuthException>(() => TrendyolResponseHandler.EnsureSuccessAsync(response));
        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
    }

    [Fact]
    public async Task EnsureSuccessAsync_With429_ThrowsTrendyolRateLimitException()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        await Assert.ThrowsAsync<TrendyolRateLimitException>(() => TrendyolResponseHandler.EnsureSuccessAsync(response));
    }

    [Fact]
    public async Task EnsureSuccessAsync_With500_ThrowsTrendyolApiException()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var ex = await Assert.ThrowsAsync<TrendyolApiException>(() => TrendyolResponseHandler.EnsureSuccessAsync(response));
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.IsNotType<TrendyolAuthException>(ex);
        Assert.IsNotType<TrendyolRateLimitException>(ex);
    }
}
