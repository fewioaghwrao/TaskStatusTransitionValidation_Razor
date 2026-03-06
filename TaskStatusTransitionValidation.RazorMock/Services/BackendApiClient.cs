using System.Net.Http.Headers;
using System.Text.Json;

namespace TaskStatusTransitionValidation.RazorMock.Services;
using TaskStatusTransitionValidation.RazorMock.Models;

public interface IBackendApiClient
{
    Task<MeResponse> GetMeAsync(string? token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectResponse>> GetProjectsAsync(string? token, CancellationToken cancellationToken = default);
}

public sealed class BackendApiClient(HttpClient httpClient) : IBackendApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<MeResponse> GetMeAsync(string? token, CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/me");
        AddBearer(req, token);

        using var res = await httpClient.SendAsync(req, cancellationToken);
        await EnsureSuccessAsync(res);

        var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        var data = await JsonSerializer.DeserializeAsync<MeResponse>(stream, JsonOptions, cancellationToken);

        return data ?? throw new InvalidOperationException("users/me のレスポンスが空です。");
    }

    public async Task<IReadOnlyList<ProjectResponse>> GetProjectsAsync(string? token, CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/projects");
        AddBearer(req, token);

        using var res = await httpClient.SendAsync(req, cancellationToken);
        await EnsureSuccessAsync(res);

        var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        var data = await JsonSerializer.DeserializeAsync<List<ProjectResponse>>(stream, JsonOptions, cancellationToken);

        return data ?? [];
    }

    private static void AddBearer(HttpRequestMessage req, string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode) return;

        var text = await res.Content.ReadAsStringAsync();

        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("認証に失敗しました。再ログインしてください。");
        }

        throw new HttpRequestException(
            $"API呼び出しに失敗しました。Status={(int)res.StatusCode} {res.StatusCode}. Body={text}");
    }
}