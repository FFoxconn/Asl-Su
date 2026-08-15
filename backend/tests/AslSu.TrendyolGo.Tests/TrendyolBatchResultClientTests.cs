using System.Net;
using System.Net.Http.Json;
using AslSu.TrendyolGo.BatchRequests;
using AslSu.TrendyolGo.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class TrendyolBatchResultClientTests
{
    private static TrendyolBatchResultClient CreateClient(
        FakeHttpMessageHandler fake, string batchResultEndpointPath = "/batch-requests/{batchRequestId}")
    {
        var httpClient = new HttpClient(fake) { BaseAddress = new Uri("https://example.invalid") };
        var options = Options.Create(new TrendyolGoOptions
        {
            SupplierId = "123456", ApiKey = "key", ApiSecret = "secret",
            BaseUrl = "https://example.invalid", BatchResultEndpointPath = batchResultEndpointPath,
        });
        return new TrendyolBatchResultClient(httpClient, options);
    }

    [Fact]
    public async Task GetBatchRequestResultAsync_WhenNotConfigured_FailsWithoutCallingNetwork()
    {
        var called = false;
        var fake = new FakeHttpMessageHandler(_ => { called = true; return new HttpResponseMessage(HttpStatusCode.OK); });
        var client = CreateClient(fake, batchResultEndpointPath: "");

        var result = await client.GetBatchRequestResultAsync("batch-1");

        Assert.False(result.Success);
        Assert.False(called);
    }

    [Fact]
    public async Task GetBatchRequestResultAsync_ReplacesBatchRequestIdPlaceholderInPath()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { status = "completed", successCount = 1, failureCount = 0 }),
        });
        var client = CreateClient(fake);

        await client.GetBatchRequestResultAsync("batch-abc-123");

        var sent = Assert.Single(fake.Requests);
        Assert.Equal("/batch-requests/batch-abc-123", sent.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetBatchRequestResultAsync_ParsesCompletedStatusAndCounts()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                status = "completed",
                successCount = 984,
                failureCount = 16,
                failureReasons = new[] { "Invalid barcode", "Missing category" },
            }),
        });
        var client = CreateClient(fake);

        var result = await client.GetBatchRequestResultAsync("batch-1");

        Assert.True(result.Success);
        Assert.Equal(TrendyolBatchResultStatus.Completed, result.Status);
        Assert.Equal(984, result.SuccessCount);
        Assert.Equal(16, result.FailureCount);
        Assert.Equal(2, result.FailureReasons.Count);
    }

    [Fact]
    public async Task GetBatchRequestResultAsync_ParsesProcessingStatus()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { status = "processing" }),
        });
        var client = CreateClient(fake);

        var result = await client.GetBatchRequestResultAsync("batch-1");

        Assert.True(result.Success);
        Assert.Equal(TrendyolBatchResultStatus.Processing, result.Status);
    }

    [Fact]
    public async Task GetBatchRequestResultAsync_WithUnparseableBody_FallsBackToUnknownRatherThanThrowing()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not json at all"),
        });
        var client = CreateClient(fake);

        var result = await client.GetBatchRequestResultAsync("batch-1");

        Assert.True(result.Success);
        Assert.Equal(TrendyolBatchResultStatus.Unknown, result.Status);
        Assert.Equal(0, result.SuccessCount);
    }

    [Fact]
    public async Task GetBatchRequestResultAsync_With401_ReturnsCredentialHint()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = CreateClient(fake);

        var result = await client.GetBatchRequestResultAsync("batch-1");

        Assert.False(result.Success);
        Assert.Equal("API Key / API Secret / Supplier ID bilgilerini kontrol edin.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetBatchRequestResultAsync_With500_ReturnsServiceErrorMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = CreateClient(fake);

        var result = await client.GetBatchRequestResultAsync("batch-1");

        Assert.False(result.Success);
        Assert.Equal("Trendyol Go servisinde geçici hata.", result.ErrorMessage);
    }
}
