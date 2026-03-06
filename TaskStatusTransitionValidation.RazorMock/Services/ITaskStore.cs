using TaskStatusTransitionValidation.RazorMock.Models;

namespace TaskStatusTransitionValidation.RazorMock.Services;

public interface ITaskStore
{
    Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken cancellationToken = default);
}