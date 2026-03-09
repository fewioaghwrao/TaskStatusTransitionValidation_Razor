namespace TaskStatusTransitionValidation.RazorMock.Models;

public sealed class TaskResponse
{
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;      // ToDo / Doing / Blocked / Done
    public string Priority { get; set; } = string.Empty;    // High / Medium / Low
    public string? DueDate { get; set; }                    // "2026-03-09" 形式を想定
    public int? AssigneeUserId { get; set; }
}