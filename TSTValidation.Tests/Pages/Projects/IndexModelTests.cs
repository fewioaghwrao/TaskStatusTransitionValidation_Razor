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

public class IndexModelTests
{
    [Fact]
    public async Task OnGetAsync_WithoutAuthToken_RedirectsToLogin()
    {
        var apiMock = new Mock<IApiClient>();
        var meProviderMock = new Mock<IMeProvider>();

        var model = new IndexModel(apiMock.Object, meProviderMock.Object);
        PageModelTestHelper.SetHttpContext(model);

        var result = await model.OnGetAsync(CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirect.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenMeIsNull_RedirectsToLogin()
    {
        var apiMock = new Mock<IApiClient>();
        var meProviderMock = new Mock<IMeProvider>();

        meProviderMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync((MeResponse?)null);

        var model = new IndexModel(apiMock.Object, meProviderMock.Object);
        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnGetAsync(CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirect.PageName);
    }

    [Fact]
    public async Task OnGetAsync_LoadsProjectsAndAppliesFilterAndPaging()
    {
        var apiMock = new Mock<IApiClient>();
        var meProviderMock = new Mock<IMeProvider>();

        meProviderMock.Setup(x => x.GetMeAsync("token-1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new MeResponse
                      {
                          UserId = 10,
                          DisplayName = "Naoki",
                          Email = "naoki@example.com",
                          Role = "Leader"
                      });

        apiMock.Setup(x => x.GetProjectsAsync("token-1", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<ProjectResponse>
               {
                   new() { ProjectId = 1, Name = "案件A", Description = "説明A", IsArchived = false },
                   new() { ProjectId = 2, Name = "案件B", Description = "説明B", IsArchived = false },
                   new() { ProjectId = 3, Name = "旧案件", Description = "アーカイブ", IsArchived = true }
               });

        var model = new IndexModel(apiMock.Object, meProviderMock.Object)
        {
            Q = "案件",
            Page = 1,
            PageSize = 5
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnGetAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal("Naoki", model.Me.DisplayName);
        Assert.True(model.IsLeader);

        Assert.Equal(2, model.ActiveCount);
        Assert.Equal(1, model.ArchivedCount);
        Assert.Equal(2, model.TotalActiveFiltered);
        Assert.Equal(2, model.ActivePageItems.Count);
        Assert.Single(model.ArchivedProjects);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(3, 5)]
    [InlineData(999, 5)]
    [InlineData(10, 10)]
    [InlineData(20, 20)]
    public async Task OnGetAsync_NormalizesPageSize(int inputPageSize, int expectedPageSize)
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

        apiMock.Setup(x => x.GetProjectsAsync("token-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectResponse>
            {
                new() { ProjectId = 1, Name = "案件A", Description = "説明A", IsArchived = false },
                new() { ProjectId = 2, Name = "案件B", Description = "説明B", IsArchived = false },
                new() { ProjectId = 3, Name = "案件C", Description = "説明C", IsArchived = false }
            });

        var model = new IndexModel(apiMock.Object, meProviderMock.Object)
        {
            Page = 1,
            PageSize = inputPageSize
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnGetAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal(expectedPageSize, model.PageSize);
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData("", 2)]
    [InlineData("   ", 2)]
    [InlineData("案件A", 1)]
    [InlineData("説明B", 1)]
    [InlineData("存在しない", 0)]
    public async Task OnGetAsync_FiltersByKeywordBoundaryValues(string? keyword, int expectedActiveFiltered)
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

        apiMock.Setup(x => x.GetProjectsAsync("token-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectResponse>
            {
                new() { ProjectId = 1, Name = "案件A", Description = "説明A", IsArchived = false },
                new() { ProjectId = 2, Name = "案件B", Description = "説明B", IsArchived = false },
                new() { ProjectId = 3, Name = "旧案件", Description = "アーカイブ", IsArchived = true }
            });

        var model = new IndexModel(apiMock.Object, meProviderMock.Object)
        {
            Q = keyword,
            Page = 1,
            PageSize = 5
        };

        PageModelTestHelper.SetHttpContext(model, "token-1");

        var result = await model.OnGetAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal(expectedActiveFiltered, model.TotalActiveFiltered);
    }
}