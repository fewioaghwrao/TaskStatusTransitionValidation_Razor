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

    public async Task<IReadOnlyList<ProjectMemberDto>> GetProjectMembersAsync(
    string? token,
    int projectId,
    CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/projects/{projectId}/members");

        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var res = await _http.SendAsync(req, cancellationToken);

        if (!res.IsSuccessStatusCode)
            return [];

        await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);

        var data = await JsonSerializer.DeserializeAsync<List<ProjectMemberDto>>(stream, JsonOptions, cancellationToken);

        return data ?? [];
    }

    public async Task<bool> CreateTaskAsync(
    string? token,
    TaskCreateRequest request,
    CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tasks");

        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(request);

        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(req, cancellationToken);

        return res.IsSuccessStatusCode;
    }

    public async Task<ProjectResponse?> CreateProjectAsync(
    string? token,
    ProjectCreateRequest request,
    CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/projects");

        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(request);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(req, cancellationToken);

        if (!res.IsSuccessStatusCode)
            return null;

        await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<ProjectResponse>(stream, JsonOptions, cancellationToken);
    }

    public async Task<ProjectDetailResponse?> GetProjectByIdAsync(
    string? token,
    int projectId,
    CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/projects/{projectId}");

        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var res = await _http.SendAsync(req, cancellationToken);

        if (!res.IsSuccessStatusCode)
            return null;

        await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<ProjectDetailResponse>(stream, JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskResponse>> GetTasksByProjectIdAsync(
        string? token,
        int projectId,
        CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/projects/{projectId}/tasks");

        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var res = await _http.SendAsync(req, cancellationToken);

        if (!res.IsSuccessStatusCode)
            return [];

        await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        var data = await JsonSerializer.DeserializeAsync<List<TaskResponse>>(stream, JsonOptions, cancellationToken);

        return data ?? [];
    }

    public async Task<bool> UpdateTaskAsync(
        string? token,
        int taskId,
        TaskUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/tasks/{taskId}");

        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(request);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(req, cancellationToken);

        return res.IsSuccessStatusCode;
    }

    public async Task<bool> ArchiveProjectAsync(
        string? token,
        int projectId,
        CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/projects/{projectId}");

        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var res = await _http.SendAsync(req, cancellationToken);

        return res.IsSuccessStatusCode;
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(
    string? token,
    int taskId,
    CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tasks/{taskId}");

        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var res = await _http.SendAsync(req, cancellationToken);

        if (!res.IsSuccessStatusCode)
            return null;

        await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<TaskResponse>(stream, JsonOptions, cancellationToken);
    }
}