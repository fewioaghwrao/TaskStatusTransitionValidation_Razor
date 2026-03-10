using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Text;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Pages.Projects.Tasks;
using TaskStatusTransitionValidation.RazorMock.Services;
using TaskStatusTransitionValidation.RazorMock.Tests.TestHelpers;
using Xunit;

namespace TaskStatusTransitionValidation.RazorMock.Tests.Pages.Projects.Tasks;

public class IndexModelTests
{
    [Fact]
    public async Task OnGetAsync_FiltersByKeywordStatusPriorityAndDue()
    {
        var apiMock = new Mock<IApiClient>();
        var meProviderMock = new Mock<IMeProvider>();

        meProviderMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MeResponse
            {
                UserId = 1,
                DisplayName = "Leader",
                Email = "leader@example.com",
                Role = "Leader"
            });

        apiMock.Setup(x => x.GetProjectByIdAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectDetailResponse
            {
                ProjectId = 1,
                Name = "案件A",
                IsArchived = false
            });

        apiMock.Setup(x => x.GetProjectMembersAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectMemberDto>
            {
                new() { UserId = 1, DisplayName = "Leader", Email = "leader@example.com" }
            });

        apiMock.Setup(x => x.GetTasksByProjectIdAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskResponse>
            {
                new()
                {
                    TaskId = 1,
                    Title = "API実装",
                    Description = "認証API",
                    Status = "Doing",
                    Priority = "High",
                    DueDate = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"),
                    AssigneeUserId = 1
                },
                new()
                {
                    TaskId = 2,
                    Title = "画面作成",
                    Description = "一覧画面",
                    Status = "ToDo",
                    Priority = "Medium",
                    DueDate = DateTime.Today.AddDays(20).ToString("yyyy-MM-dd"),
                    AssigneeUserId = 1
                },
                new()
                {
                    TaskId = 3,
                    Title = "バッチ修正",
                    Description = "旧機能",
                    Status = "Done",
                    Priority = "High",
                    DueDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
                    AssigneeUserId = 1
                }
            });

        var model = new IndexModel(apiMock.Object, meProviderMock.Object)
        {
            Q = "API",
            StatusF = "Doing",
            PrioF = "High",
            DueF = "DueSoon",
            PageNo = 1,
            PageSize = 10
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnGetAsync(1, CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal(1, model.ProjectId);
        Assert.Single(model.FilteredTasks);
        Assert.Single(model.PageItems);

        var item = model.PageItems.Single();
        Assert.Equal(1, item.TaskId);
        Assert.Equal("API実装", item.Title);
    }

    [Fact]
    public async Task OnGetAsync_AppliesPagingCorrectly()
    {
        var apiMock = new Mock<IApiClient>();
        var meProviderMock = new Mock<IMeProvider>();

        meProviderMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MeResponse
            {
                UserId = 1,
                DisplayName = "Worker",
                Email = "worker@example.com",
                Role = "Worker"
            });

        apiMock.Setup(x => x.GetProjectByIdAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectDetailResponse
            {
                ProjectId = 1,
                Name = "案件A",
                IsArchived = false
            });

        apiMock.Setup(x => x.GetProjectMembersAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectMemberDto>
            {
                new() { UserId = 1, DisplayName = "Worker", Email = "worker@example.com" }
            });

        apiMock.Setup(x => x.GetTasksByProjectIdAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Range(1, 12).Select(i => new TaskResponse
            {
                TaskId = i,
                Title = $"Task-{i}",
                Description = $"Desc-{i}",
                Status = "ToDo",
                Priority = "Medium",
                DueDate = DateTime.Today.AddDays(10).ToString("yyyy-MM-dd"),
                AssigneeUserId = 1
            }).ToList());

        var model = new IndexModel(apiMock.Object, meProviderMock.Object)
        {
            PageNo = 2,
            PageSize = 5
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnGetAsync(1, CancellationToken.None);

        Assert.IsType<PageResult>(result);

        Assert.Equal(12, model.TotalFiltered);
        Assert.Equal(3, model.TotalPages);
        Assert.Equal(2, model.SafePage);
        Assert.Equal(6, model.DisplayStart);
        Assert.Equal(10, model.DisplayEnd);
        Assert.Equal(5, model.PageItems.Count);

        // TaskId降順で並び替えされているので、
        // 12,11,10,9,8 が1ページ目、7,6,5,4,3 が2ページ目
        Assert.Equal(new[] { 7, 6, 5, 4, 3 }, model.PageItems.Select(x => x.TaskId).ToArray());
    }

    [Fact]
    public async Task OnPostChangeStatusAsync_WhenTransitionIsValid_RedirectsWithOperationMessage()
    {
        var apiMock = new Mock<IApiClient>();
        var meProviderMock = new Mock<IMeProvider>();

        meProviderMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MeResponse
            {
                UserId = 1,
                DisplayName = "Leader",
                Email = "leader@example.com",
                Role = "Leader"
            });

        apiMock.Setup(x => x.GetTasksByProjectIdAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskResponse>
            {
                new()
                {
                    TaskId = 10,
                    Title = "Task A",
                    Description = "desc",
                    Status = "Doing",
                    Priority = "Medium",
                    DueDate = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"),
                    AssigneeUserId = 1
                }
            });

        TaskUpdateRequest? captured = null;

        apiMock.Setup(x => x.UpdateTaskAsync("token-1", 10, It.IsAny<TaskUpdateRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string?, int, TaskUpdateRequest, CancellationToken>((_, _, req, _) => captured = req)
            .ReturnsAsync(true);

        var model = new IndexModel(apiMock.Object, meProviderMock.Object);
        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostChangeStatusAsync(
            projectId: 1,
            taskId: 10,
            nextStatus: "Done",
            q: "abc",
            statusF: "Doing",
            prioF: "Medium",
            dueF: "All",
            pageNo: 2,
            pageSize: 20,
            cancellationToken: CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Projects/Tasks/Index", redirect.PageName);

        Assert.NotNull(captured);
        Assert.Equal("Done", captured!.Status);
        Assert.Equal("Task A", captured.Title);
        Assert.Equal("desc", captured.Description);
        Assert.Equal("Medium", captured.Priority);

        Assert.Equal("タスク #10 の状態を 完了 に更新しました。", model.OperationMessage);
    }

    [Fact]
    public async Task OnPostChangeStatusAsync_WhenTransitionIsInvalid_RedirectsWithErrorMessage()
    {
        var apiMock = new Mock<IApiClient>();
        var meProviderMock = new Mock<IMeProvider>();

        meProviderMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MeResponse
            {
                UserId = 1,
                DisplayName = "Leader",
                Email = "leader@example.com",
                Role = "Leader"
            });

        apiMock.Setup(x => x.GetTasksByProjectIdAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskResponse>
            {
                new()
                {
                    TaskId = 10,
                    Title = "Task A",
                    Description = "desc",
                    Status = "ToDo",
                    Priority = "Medium",
                    DueDate = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"),
                    AssigneeUserId = 1
                }
            });

        var model = new IndexModel(apiMock.Object, meProviderMock.Object);
        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostChangeStatusAsync(
            projectId: 1,
            taskId: 10,
            nextStatus: "Done",
            q: null,
            statusF: null,
            prioF: null,
            dueF: null,
            pageNo: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Projects/Tasks/Index", redirect.PageName);

        Assert.Equal("状態遷移が許可されていません: ToDo → Done", model.ErrorMessage);

        apiMock.Verify(
            x => x.UpdateTaskAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<TaskUpdateRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostExportCsvAsync_ReturnsCsvFile()
    {
        var apiMock = new Mock<IApiClient>();
        var meProviderMock = new Mock<IMeProvider>();

        meProviderMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MeResponse
            {
                UserId = 1,
                DisplayName = "Leader",
                Email = "leader@example.com",
                Role = "Leader"
            });

        apiMock.Setup(x => x.GetProjectByIdAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectDetailResponse
            {
                ProjectId = 1,
                Name = "案件A",
                IsArchived = false
            });

        apiMock.Setup(x => x.GetProjectMembersAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectMemberDto>
            {
                new() { UserId = 1, DisplayName = "Leader", Email = "leader@example.com" }
            });

        apiMock.Setup(x => x.GetTasksByProjectIdAsync("token-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskResponse>
            {
                new()
                {
                    TaskId = 100,
                    Title = "CSV確認",
                    Description = "出力テスト",
                    Status = "Doing",
                    Priority = "High",
                    DueDate = "2026-03-15",
                    AssigneeUserId = 1
                }
            });

        var model = new IndexModel(apiMock.Object, meProviderMock.Object);
        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnPostExportCsvAsync(
            projectId: 1,
            q: null,
            statusF: "All",
            prioF: "All",
            dueF: "All",
            pageNo: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv; charset=utf-8", fileResult.ContentType);
        Assert.EndsWith(".csv", fileResult.FileDownloadName);

        var csvText = Encoding.UTF8.GetString(fileResult.FileContents);

        Assert.Contains("TaskId", csvText);
        Assert.Contains("Title", csvText);
        Assert.Contains("CSV確認", csvText);
        Assert.Contains("作業中", csvText);
        Assert.Contains("Leader", csvText);
    }
}