using System.Net;
using System.Text;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Services;
using TaskStatusTransitionValidation.RazorMock.Tests.TestHelpers;
using Xunit;

namespace TaskStatusTransitionValidation.RazorMock.Tests.Services;

public class ApiClientTests
{
    private static ApiClient CreateClient(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test")
        };

        return new ApiClient(httpClient);
    }

    [Fact]
    public async Task LoginAsync_Success_ReturnsLoginResponse()
    {
        string? capturedBody = null;

        var handler = new FakeHttpMessageHandler(async (request, _) =>
        {
            capturedBody = await request.Content!.ReadAsStringAsync();

            return FakeHttpMessageHandler.JsonResponse("""
            {
              "token":"test-token"
            }
            """);
        });

        var client = CreateClient(handler);

        var request = new LoginRequest
        {
            Email = "naoki@example.com",
            Password = "password123"
        };

        var result = await client.LoginAsync(request);

        Assert.NotNull(result);
        Assert.Equal("test-token", result!.Token);

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/api/v1/auth/login", handler.LastRequest.RequestUri!.AbsolutePath);

        Assert.Contains("naoki@example.com", capturedBody);
        Assert.Contains("password123", capturedBody);
    }

    [Fact]
    public async Task GetMeAsync_Success_ReturnsUser()
    {
        var handler = new FakeHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/v1/users/me", request.RequestUri!.AbsolutePath);

            var auth = request.Headers.Authorization;
            Assert.Equal("Bearer", auth!.Scheme);
            Assert.Equal("token-123", auth.Parameter);

            return Task.FromResult(
                FakeHttpMessageHandler.JsonResponse("""
                {
                  "userId":1,
                  "displayName":"Naoki",
                  "email":"naoki@example.com",
                  "role":"Leader"
                }
                """));
        });

        var client = CreateClient(handler);

        var result = await client.GetMeAsync("token-123");

        Assert.NotNull(result);
        Assert.Equal(1, result!.UserId);
        Assert.Equal("Naoki", result.DisplayName);
    }

    [Fact]
    public async Task GetProjectsAsync_Success_ReturnsProjects()
    {
        var handler = new FakeHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/v1/projects", request.RequestUri!.AbsolutePath);

            return Task.FromResult(
                FakeHttpMessageHandler.JsonResponse("""
                [
                  {
                    "projectId":1,
                    "name":"案件A",
                    "description":"説明A",
                    "isArchived":false
                  }
                ]
                """));
        });

        var client = CreateClient(handler);

        var result = await client.GetProjectsAsync("token-abc");

        Assert.Single(result);
        Assert.Equal("案件A", result[0].Name);
    }

    [Fact]
    public async Task CreateTaskAsync_Success_ReturnsTrue()
    {
        string? capturedBody = null;

        var handler = new FakeHttpMessageHandler(async (request, _) =>
        {
            capturedBody = await request.Content!.ReadAsStringAsync();

            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/tasks", request.RequestUri!.AbsolutePath);

            var auth = request.Headers.Authorization;
            Assert.Equal("Bearer", auth!.Scheme);

            return FakeHttpMessageHandler.EmptyResponse(HttpStatusCode.Created);
        });

        var client = CreateClient(handler);

        var request = new TaskCreateRequest
        {
            ProjectId = 1,
            Title = "Task A",
            Description = "Create first task",
            Priority = TaskPriority.High
        };

        var result = await client.CreateTaskAsync("token-1", request);

        Assert.True(result);

        Assert.Contains("\"ProjectId\":1", capturedBody);
        Assert.Contains("\"Title\":\"Task A\"", capturedBody);
        Assert.Contains("\"Description\":\"Create first task\"", capturedBody);
    }

    [Fact]
    public async Task UpdateTaskAsync_Success_ReturnsTrue()
    {
        string? capturedBody = null;

        var handler = new FakeHttpMessageHandler(async (request, _) =>
        {
            capturedBody = await request.Content!.ReadAsStringAsync();

            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal("/api/v1/tasks/99", request.RequestUri!.AbsolutePath);

            return FakeHttpMessageHandler.EmptyResponse(HttpStatusCode.OK);
        });

        var client = CreateClient(handler);

        var request = new TaskUpdateRequest
        {
            Title = "Updated Task",
            Description = "Updated description",
            Priority = "High",
            Status = "Done"
        };

        var result = await client.UpdateTaskAsync("token-1", 99, request);

        Assert.True(result);

        Assert.Contains("\"Title\":\"Updated Task\"", capturedBody);
        Assert.Contains("\"Status\":\"Done\"", capturedBody);
    }
}