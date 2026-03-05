using System.Collections.Concurrent;

namespace TaskStatusTransitionValidation.RazorMock.Services;

public enum UserRole { Leader, Worker }
public enum TaskStatus { ToDo, Doing, Blocked, Done }

public sealed record MeDto(string DisplayName, UserRole Role);

public sealed record ProjectDto(int Id, string Name);
public sealed record TaskDto(int Id, int ProjectId, string Title, TaskStatus Status, string? Assignee);

public interface IMeProvider
{
    MeDto GetMe();
    void SetMe(MeDto me);
}

public sealed class MockMeProvider : IMeProvider
{
    private MeDto _me = new("Mock User", UserRole.Leader);
    public MeDto GetMe() => _me;
    public void SetMe(MeDto me) => _me = me;
}

public interface ITaskStore
{
    IReadOnlyList<ProjectDto> GetProjects();
    IReadOnlyList<TaskDto> GetTasks(int projectId);
    TaskDto? GetTask(int id);
    void CreateTask(TaskDto task);
    void UpdateTask(TaskDto task);
}

public sealed class MockTaskStore : ITaskStore
{
    private readonly List<ProjectDto> _projects =
    [
        new(1, "Demo Project A"),
        new(2, "Demo Project B"),
    ];

    private readonly ConcurrentDictionary<int, TaskDto> _tasks = new(
        new[]
        {
            new KeyValuePair<int, TaskDto>(10, new TaskDto(10, 1, "Spec review", TaskStatus.ToDo, "Naoki")),
            new KeyValuePair<int, TaskDto>(11, new TaskDto(11, 1, "Implement UI", TaskStatus.Doing, "Naoki")),
            new KeyValuePair<int, TaskDto>(12, new TaskDto(12, 2, "Write tests", TaskStatus.Blocked, null)),
        });

    public IReadOnlyList<ProjectDto> GetProjects() => _projects;

    public IReadOnlyList<TaskDto> GetTasks(int projectId)
        => _tasks.Values.Where(x => x.ProjectId == projectId).OrderBy(x => x.Id).ToList();

    public TaskDto? GetTask(int id) => _tasks.TryGetValue(id, out var t) ? t : null;

    public void CreateTask(TaskDto task) => _tasks[task.Id] = task;

    public void UpdateTask(TaskDto task) => _tasks[task.Id] = task;
}