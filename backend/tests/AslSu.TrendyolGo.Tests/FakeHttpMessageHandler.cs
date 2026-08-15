namespace AslSu.TrendyolGo.Tests;

/// <summary>Minimal hand-rolled fake HttpMessageHandler for these tests, so no extra mocking
/// package is needed just to stub HTTP responses.</summary>
public class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(respond(request));
    }
}
