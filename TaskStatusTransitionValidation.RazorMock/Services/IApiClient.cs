using TaskStatusTransitionValidation.RazorMock.Models;

namespace TaskStatusTransitionValidation.RazorMock.Services;

public interface IApiClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<MeResponse?> GetMeAsync(string token, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectResponse>> GetProjectsAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<ProjectDetailResponse?> GetProjectByIdAsync(
        string token,
        int projectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectMemberDto>> GetProjectMembersAsync(
        string token,
        int projectId,
        CancellationToken cancellationToken = default);

    Task<ProjectResponse?> CreateProjectAsync(
        string token,
        ProjectCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveProjectAsync(
        string token,
        int projectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskResponse>> GetTasksByProjectIdAsync(
        string token,
        int projectId,
        CancellationToken cancellationToken = default);

    Task<TaskResponse?> GetTaskByIdAsync(
        string token,
        int taskId,
        CancellationToken cancellationToken = default);

    Task<bool> CreateTaskAsync(
        string token,
        TaskCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateTaskAsync(
        string token,
        int taskId,
        TaskUpdateRequest request,
        CancellationToken cancellationToken = default);
}