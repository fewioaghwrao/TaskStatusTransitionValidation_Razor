using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using TaskStatusTransitionValidation.RazorMock.Pages;
using TaskStatusTransitionValidation.RazorMock.Services;
using TaskStatusTransitionValidation.RazorMock.Tests.TestHelpers;
using Xunit;

namespace TaskStatusTransitionValidation.RazorMock.Tests.Pages;

public class LoginModelTests
{
    [Fact]
    public async Task OnPostAsync_ModelStateInvalid_ReturnsPage()
    {
        var apiMock = new Mock<IApiClient>();
        var model = new LoginModel(apiMock.Object);

        PageModelTestHelper.SetHttpContext(model);
        model.ModelState.AddModelError("Input.Email", "必須");

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        apiMock.Verify(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_LoginFailed_ReturnsPageAndSetsErrorMessage()
    {
        var apiMock = new Mock<IApiClient>();
        apiMock.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((LoginResponse?)null);

        var model = new LoginModel(apiMock.Object)
        {
            Input = new LoginRequest
            {
                Email = "test@example.com",
                Password = "password"
            }
        };

        PageModelTestHelper.SetHttpContext(model);

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("ログインに失敗しました。入力内容をご確認ください。", model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_LoginSucceeded_RedirectsToProjectsIndex_AndSetsCookie()
    {
        var apiMock = new Mock<IApiClient>();
        apiMock.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new LoginResponse { Token = "test-token" });

        var model = new LoginModel(apiMock.Object)
        {
            Input = new LoginRequest
            {
                Email = "test@example.com",
                Password = "password"
            }
        };

        PageModelTestHelper.SetHttpContext(model);

        var result = await model.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Projects/Index", redirect.PageName);

        var setCookie = model.Response.Headers.SetCookie.ToString();
        Assert.Contains("auth_token=test-token", setCookie);
    }
}