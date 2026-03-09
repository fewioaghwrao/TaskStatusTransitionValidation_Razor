using TaskStatusTransitionValidation.RazorMock.Models;

namespace TaskStatusTransitionValidation.RazorMock.Services;

public sealed class InMemoryTaskStore : ITaskStore
{
    private static readonly IReadOnlyList<ProjectDto> _projects =
    [
        new ProjectDto
        {
            Id = 0,
            Name = "案件A",
            Description = "A",
            IsArchived = true
        },
        new ProjectDto
        {
            Id = 1,
            Name = "案件B",
            Description = "B",
            IsArchived = true
        }
    ];

    public Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_projects);
}