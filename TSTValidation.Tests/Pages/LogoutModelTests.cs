using Microsoft.AspNetCore.Mvc;
using TaskStatusTransitionValidation.RazorMock.Pages;
using TaskStatusTransitionValidation.RazorMock.Tests.TestHelpers;
using Xunit;

namespace TaskStatusTransitionValidation.RazorMock.Tests.Pages;

public class LogoutModelTests
{
    [Fact]
    public void OnPost_RedirectsToLogin()
    {
        var model = new LogoutModel();
        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = model.OnPost();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirect.PageName);
    }
}
