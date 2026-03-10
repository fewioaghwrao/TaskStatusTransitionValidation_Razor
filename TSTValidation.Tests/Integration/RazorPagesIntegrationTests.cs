using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TaskStatusTransitionValidation.RazorMock.Tests.Integration;

public sealed class RazorPagesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RazorPagesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(bool allowAutoRedirect = false)
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = allowAutoRedirect,
            HandleCookies = true
        });
    }

    [Fact]
    public async Task Get_Login_ReturnsSuccessAndHtml()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType!.ToString());

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("ログイン", html);
    }

    [Fact]
    public async Task Get_ProjectsIndex_WithoutAuthCookie_RedirectsToLogin()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/Projects");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.StartsWith("/Login", response.Headers.Location!.OriginalString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Get_ProjectsIndex_AfterLogin_ReturnsSuccess()
    {
        var client = CreateClient();

        var (_, token) = await AntiforgeryHelper.GetFormInfoAsync(client, "/Login");

        var loginForm = new Dictionary<string, string>
        {
            ["Input.Email"] = "naoki@example.com",
            ["Input.Password"] = "password123",
            ["__RequestVerificationToken"] = token
        };

        var loginResponse = await client.PostAsync("/Login", new FormUrlEncodedContent(loginForm));

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
        Assert.NotNull(loginResponse.Headers.Location);
        Assert.Equal("/Projects", loginResponse.Headers.Location!.OriginalString);

        var response = await client.GetAsync("/Projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("案件", html);
    }

    [Fact]
    public async Task Get_ProjectTasksIndex_AfterLogin_ReturnsSuccess()
    {
        var client = CreateClient();

        var (_, token) = await AntiforgeryHelper.GetFormInfoAsync(client, "/Login");

        var loginForm = new Dictionary<string, string>
        {
            ["Input.Email"] = "naoki@example.com",
            ["Input.Password"] = "password123",
            ["__RequestVerificationToken"] = token
        };

        var loginResponse = await client.PostAsync("/Login", new FormUrlEncodedContent(loginForm));

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
        Assert.NotNull(loginResponse.Headers.Location);
        Assert.Equal("/Projects", loginResponse.Headers.Location!.OriginalString);

        var response = await client.GetAsync("/Projects/1/Tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("タスク", html);
    }

    [Fact]
    public async Task Post_Login_WithValidCredentials_RedirectsToProjectsIndex()
    {
        var client = CreateClient();

        var (_, token) = await AntiforgeryHelper.GetFormInfoAsync(client, "/Login");

        var form = new Dictionary<string, string>
        {
            ["Input.Email"] = "naoki@example.com",
            ["Input.Password"] = "password123",
            ["__RequestVerificationToken"] = token
        };

        var response = await client.PostAsync("/Login", new FormUrlEncodedContent(form));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/Projects", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Post_Logout_RedirectsToLogin()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "auth_token=test-token");

        var (_, token) = await AntiforgeryHelper.GetFormInfoAsync(client, "/Projects");

        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        };

        var response = await client.PostAsync("/Logout", new FormUrlEncodedContent(form));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/Login", response.Headers.Location!.OriginalString);
    }
}