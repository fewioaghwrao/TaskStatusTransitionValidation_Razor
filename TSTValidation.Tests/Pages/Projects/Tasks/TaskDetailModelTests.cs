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

public class TaskDetailModelTests
{
    [Fact]
    public async Task OnPostAsync_WhenOriginalTaskIsDone_ReturnsPageWithError()
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
               .ReturnsAsync(new List<ProjectMemberDto>());

        apiMock.Setup(x => x.GetTaskByIdAsync("token-1", 10, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new TaskResponse
               {
                   TaskId = 10,
                   Title = "完了済みタスク",
                   Description = "desc",
                   DueDate = "2026-03-20",
                   Priority = "Medium",
                   Status = "Done",
                   AssigneeUserId = 1
               });

        var model = new TaskDetailModel(apiMock.Object)
        {
            Input = new TaskDetailModel.TaskEditInputModel
            {
                Title = "更新後",
                Description = "更新後説明",
                DueDate = "2026-03-21",
                Priority = "High",
                Status = "Done"
            },
            ConfirmSubmit = true
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostAsync(1, 10, CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal("完了（Done）のタスクは更新できません。", model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenTransitionIsInvalid_ReturnsPageWithError()
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
               .ReturnsAsync(new List<ProjectMemberDto>());

        apiMock.Setup(x => x.GetTaskByIdAsync("token-1", 10, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new TaskResponse
               {
                   TaskId = 10,
                   Title = "タスクA",
                   Description = "desc",
                   DueDate = "2026-03-20",
                   Priority = "Medium",
                   Status = "ToDo",
                   AssigneeUserId = 1
               });

        var model = new TaskDetailModel(apiMock.Object)
        {
            Input = new TaskDetailModel.TaskEditInputModel
            {
                Title = "タスクA更新",
                Description = "説明更新",
                DueDate = "2026-03-21",
                Priority = "High",
                Status = "Done"
            },
            ConfirmSubmit = true
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostAsync(1, 10, CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal("状態遷移が許可されていません: ToDo → Done", model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenValid_RedirectsToTaskIndex()
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
               .ReturnsAsync(new List<ProjectMemberDto>());

        apiMock.Setup(x => x.GetTaskByIdAsync("token-1", 10, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new TaskResponse
               {
                   TaskId = 10,
                   Title = "タスクA",
                   Description = "desc",
                   DueDate = "2026-03-20",
                   Priority = "Medium",
                   Status = "Doing",
                   AssigneeUserId = 1
               });

        apiMock.Setup(x => x.UpdateTaskAsync("token-1", 10, It.IsAny<TaskUpdateRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var model = new TaskDetailModel(apiMock.Object)
        {
            Input = new TaskDetailModel.TaskEditInputModel
            {
                Title = " タスクA更新 ",
                Description = " 説明更新 ",
                DueDate = "2026-03-21",
                Priority = "High",
                Status = "Done"
            },
            ConfirmSubmit = true
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostAsync(1, 10, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Projects/Tasks/Index", redirect.PageName);
    }
}