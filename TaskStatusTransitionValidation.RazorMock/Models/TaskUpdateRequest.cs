namespace TaskStatusTransitionValidation.RazorMock.Models;

public sealed class TaskUpdateRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AssigneeUserId { get; set; }
    public string? DueDate { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}