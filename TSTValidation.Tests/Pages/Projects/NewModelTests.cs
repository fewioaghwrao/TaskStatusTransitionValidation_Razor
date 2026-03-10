using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Net;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Pages.Projects;
using TaskStatusTransitionValidation.RazorMock.Services;
using TaskStatusTransitionValidation.RazorMock.Tests.TestHelpers;
using Xunit;

namespace TaskStatusTransitionValidation.RazorMock.Tests.Pages.Projects;

public class NewModelTests
{
    [Fact]
    public async Task OnGetAsync_WhenNotLeader_RedirectsToProjectsIndex()
    {
        var apiMock = new Mock<IApiClient>();
        apiMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new MeResponse
               {
                   UserId = 1,
                   DisplayName = "Worker User",
                   Email = "worker@example.com",
                   Role = "Worker"
               });

        var model = new NewModel(apiMock.Object);
        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnGetAsync(CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Projects/Index", redirect.PageName);
    }

    [Fact]
    public async Task OnPostAsync_EmptyName_ReturnsPageWithError()
    {
        var apiMock = new Mock<IApiClient>();
        apiMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new MeResponse
               {
                   UserId = 1,
                   DisplayName = "Leader User",
                   Email = "leader@example.com",
                   Role = "Leader"
               });

        var model = new NewModel(apiMock.Object)
        {
            Input = new ProjectCreateRequest
            {
                Name = "   ",
            }
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal("プロジェクト名を入力してください。", model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_LeaderAndValidInput_RedirectsToProjectsIndex()
    {
        var apiMock = new Mock<IApiClient>();
        apiMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new MeResponse
               {
                   UserId = 1,
                   DisplayName = "Leader User",
                   Email = "leader@example.com",
                   Role = "Leader"
               });

        apiMock.Setup(x => x.CreateProjectAsync("token-1", It.IsAny<ProjectCreateRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ProjectResponse
               {
                   ProjectId = 100,
                   Name = "新規案件",
                   Description = "説明",
                   IsArchived = false
               });

        var model = new NewModel(apiMock.Object)
        {
            Input = new ProjectCreateRequest
            {
                Name = " 新規案件 ",
            }
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostAsync(CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Projects/Index", redirect.PageName);
        Assert.Equal("新規案件", model.Input.Name);
    }
}