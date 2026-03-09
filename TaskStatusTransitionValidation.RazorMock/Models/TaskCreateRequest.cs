namespace TaskStatusTransitionValidation.RazorMock.Models;

public enum TaskPriority
{
    High = 1,
    Medium = 2,
    Low = 3
}

public sealed class TaskCreateRequest
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int? AssigneeUserId { get; set; }
    public DateOnly? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}