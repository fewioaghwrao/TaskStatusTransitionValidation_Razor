using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Services;

namespace TaskStatusTransitionValidation.RazorMock.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IApiClient>();
            services.RemoveAll<IMeProvider>();

            services.AddSingleton<IApiClient, FakeApiClient>();
            services.AddSingleton<IMeProvider, FakeMeProvider>();
        });
    }
}

internal sealed class FakeMeProvider : IMeProvider
{
    public Task<MeResponse?> GetMeAsync(string? token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || token != "test-token")
        {
            return Task.FromResult<MeResponse?>(null);
        }

        return Task.FromResult<MeResponse?>(new MeResponse
        {
            UserId = 1,
            DisplayName = "Naoki",
            Email = "naoki@example.com",
            Role = "Leader"
        });
    }
}

internal sealed class FakeApiClient : IApiClient
{
    public Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Email == "naoki@example.com" && request.Password == "password123")
        {
            return Task.FromResult<LoginResponse?>(new LoginResponse
            {
                Token = "test-token"
            });
        }

        return Task.FromResult<LoginResponse?>(null);
    }

    public Task<MeResponse?> GetMeAsync(string? token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || token != "test-token")
        {
            return Task.FromResult<MeResponse?>(null);
        }

        return Task.FromResult<MeResponse?>(new MeResponse
        {
            UserId = 1,
            DisplayName = "Naoki",
            Email = "naoki@example.com",
            Role = "Leader"
        });
    }

    public Task<IReadOnlyList<ProjectResponse>> GetProjectsAsync(string? token, CancellationToken cancellationToken = default)
    {
        if (token != "test-token")
        {
            return Task.FromResult<IReadOnlyList<ProjectResponse>>([]);
        }

        IReadOnlyList<ProjectResponse> data =
        [
            new ProjectResponse
            {
                ProjectId = 1,
                Name = "案件A",
                Description = "説明A",
                IsArchived = false
            },
            new ProjectResponse
            {
                ProjectId = 2,
                Name = "旧案件",
                Description = "説明B",
                IsArchived = true
            }
        ];

        return Task.FromResult(data);
    }

    public Task<IReadOnlyList<ProjectMemberDto>> GetProjectMembersAsync(
        string? token,
        int projectId,
        CancellationToken cancellationToken = default)
    {
        if (token != "test-token")
        {
            return Task.FromResult<IReadOnlyList<ProjectMemberDto>>([]);
        }

        IReadOnlyList<ProjectMemberDto> data =
        [
            new ProjectMemberDto
            {
                UserId = 1,
                DisplayName = "Naoki",
                Email = "naoki@example.com"
            }
        ];

        return Task.FromResult(data);
    }

    public Task<bool> CreateTaskAsync(
        string? token,
        TaskCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(token == "test-token");
    }

    public Task<ProjectResponse?> CreateProjectAsync(
        string? token,
        ProjectCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (token != "test-token")
        {
            return Task.FromResult<ProjectResponse?>(null);
        }

        return Task.FromResult<ProjectResponse?>(new ProjectResponse
        {
            ProjectId = 99,
            Name = request.Name,
            Description = "テスト",
            IsArchived = false
        });
    }

    public Task<ProjectDetailResponse?> GetProjectByIdAsync(
        string? token,
        int projectId,
        CancellationToken cancellationToken = default)
    {
        if (token != "test-token")
        {
            return Task.FromResult<ProjectDetailResponse?>(null);
        }

        return Task.FromResult<ProjectDetailResponse?>(new ProjectDetailResponse
        {
            ProjectId = projectId,
            Name = "案件A",
            IsArchived = false
        });
    }

    public Task<IReadOnlyList<TaskResponse>> GetTasksByProjectIdAsync(
        string? token,
        int projectId,
        CancellationToken cancellationToken = default)
    {
        if (token != "test-token")
        {
            return Task.FromResult<IReadOnlyList<TaskResponse>>([]);
        }

        IReadOnlyList<TaskResponse> data =
        [
            new TaskResponse
            {
                TaskId = 10,
                Title = "API実装",
                Description = "認証API",
                Status = "Doing",
                Priority = "High",
                DueDate = "2026-03-31",
                AssigneeUserId = 1
            },
            new TaskResponse
            {
                TaskId = 11,
                Title = "画面作成",
                Description = "一覧画面",
                Status = "ToDo",
                Priority = "Medium",
                DueDate = "2026-04-05",
                AssigneeUserId = 1
            }
        ];

        return Task.FromResult(data);
    }

    public Task<bool> UpdateTaskAsync(
        string? token,
        int taskId,
        TaskUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(token == "test-token");
    }

    public Task<bool> ArchiveProjectAsync(
        string? token,
        int projectId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(token == "test-token");
    }

    public Task<TaskResponse?> GetTaskByIdAsync(
        string? token,
        int taskId,
        CancellationToken cancellationToken = default)
    {
        if (token != "test-token")
        {
            return Task.FromResult<TaskResponse?>(null);
        }

        return Task.FromResult<TaskResponse?>(new TaskResponse
        {
            TaskId = taskId,
            Title = "API実装",
            Description = "認証API",
            Status = "Doing",
            Priority = "High",
            DueDate = "2026-03-31",
            AssigneeUserId = 1
        });
    }
}