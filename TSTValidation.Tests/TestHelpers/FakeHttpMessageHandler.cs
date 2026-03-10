using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TaskStatusTransitionValidation.RazorMock.Tests.TestHelpers;

public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

    public HttpRequestMessage? LastRequest { get; private set; }

    public FakeHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        return await _handler(request, cancellationToken);
    }

    public static HttpResponseMessage JsonResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
    }

    public static HttpResponseMessage EmptyResponse(HttpStatusCode statusCode)
    {
        return new HttpResponseMessage(statusCode);
    }
}
