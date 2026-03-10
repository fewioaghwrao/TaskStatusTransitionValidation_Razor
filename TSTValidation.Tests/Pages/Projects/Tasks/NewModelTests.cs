using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Net;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Pages.Projects.Tasks;
using TaskStatusTransitionValidation.RazorMock.Services;
using TaskStatusTransitionValidation.RazorMock.Tests.TestHelpers;
using Xunit;

namespace TaskStatusTransitionValidation.RazorMock.Tests.Pages.Projects.Tasks;

public class NewModelTests
{
    [Fact]
    public async Task OnPostAsync_WhenUserIsNotMember_ReturnsPageWithError()
    {
        var apiMock = new Mock<IApiClient>();

        apiMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new MeResponse
               {
                   UserId = 99,
                   DisplayName = "Other User",
                   Email = "other@example.com",
                   Role = "Worker"
               });

        apiMock.Setup(x => x.GetProjectMembersAsync("token-1", 1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<ProjectMemberDto>
               {
                   new() { UserId = 1, DisplayName = "Member1", Email = "m1@example.com" }
               });

        var model = new NewModel(apiMock.Object)
        {
            Input = new NewModel.TaskCreateInputModel
            {
                ProjectId = 1,
                Title = "新規タスク",
                Priority = TaskPriority.Medium
            },
            ConfirmSubmit = true
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostAsync(1, CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal("この案件のメンバーではないため、タスクを作成できません。", model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenConfirmSubmitIsFalse_ReturnsPageWithError()
    {
        var apiMock = new Mock<IApiClient>();

        apiMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new MeResponse
               {
                   UserId = 1,
                   DisplayName = "Leader",
                   Email = "leader@example.com",
                   Role = "Leader"
               });

        apiMock.Setup(x => x.GetProjectMembersAsync("token-1", 1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<ProjectMemberDto>
               {
                   new() { UserId = 1, DisplayName = "Leader", Email = "leader@example.com" }
               });

        var model = new NewModel(apiMock.Object)
        {
            Input = new NewModel.TaskCreateInputModel
            {
                ProjectId = 1,
                Title = "新規タスク",
                Priority = TaskPriority.High,
                AssigneeUserId = 1
            },
            ConfirmSubmit = false
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostAsync(1, CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal("確認ダイアログから作成を確定してください。", model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_Worker_AssigneeIsNormalizedToNull_AndRedirectsOnSuccess()
    {
        var apiMock = new Mock<IApiClient>();

        apiMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new MeResponse
               {
                   UserId = 2,
                   DisplayName = "Worker",
                   Email = "worker@example.com",
                   Role = "Worker"
               });

        apiMock.Setup(x => x.GetProjectMembersAsync("token-1", 1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<ProjectMemberDto>
               {
                   new() { UserId = 2, DisplayName = "Worker", Email = "worker@example.com" }
               });

        TaskCreateRequest? capturedRequest = null;

        apiMock.Setup(x => x.CreateTaskAsync("token-1", It.IsAny<TaskCreateRequest>(), It.IsAny<CancellationToken>()))
               .Callback<string?, TaskCreateRequest, CancellationToken>((_, req, _) => capturedRequest = req)
               .ReturnsAsync(true);

        var model = new NewModel(apiMock.Object)
        {
            Input = new NewModel.TaskCreateInputModel
            {
                ProjectId = 1,
                Title = " タスクA ",
                Description = " 説明 ",
                Priority = TaskPriority.Medium,
                AssigneeUserId = 999
            },
            ConfirmSubmit = true
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostAsync(1, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Projects/Tasks/Index", redirect.PageName);

        Assert.NotNull(capturedRequest);
        Assert.Null(capturedRequest!.AssigneeUserId);
        Assert.Equal("タスクA", capturedRequest.Title);
        Assert.Equal("説明", capturedRequest.Description);
    }
}