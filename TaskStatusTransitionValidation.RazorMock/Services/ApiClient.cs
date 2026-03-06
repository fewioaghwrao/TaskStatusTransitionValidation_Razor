using Microsoft.AspNetCore.Identity.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskStatusTransitionValidation.RazorMock.Models;

namespace TaskStatusTransitionValidation.RazorMock.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var res = await _http.PostAsync("/api/v1/auth/login", content, cancellationToken);

        if (!res.IsSuccessStatusCode)
            return null;

        await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<LoginResponse>(stream, JsonOptions, cancellationToken);
    }

    public async Task<MeResponse?> GetMeAsync(string? token, CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/me");

        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var res = await _http.SendAsync(req, cancellationToken);

        if (!res.IsSuccessStatusCode)
            return null;

        await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<MeResponse>(stream, JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectResponse>> GetProjectsAsync(string? token, CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/projects");

        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var res = await _http.SendAsync(req, cancellationToken);

        if (!res.IsSuccessStatusCode)
            return [];

        await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        var data = await JsonSerializer.DeserializeAsync<List<ProjectResponse>>(stream, JsonOptions, cancellationToken);

        return data ?? [];
    }
}