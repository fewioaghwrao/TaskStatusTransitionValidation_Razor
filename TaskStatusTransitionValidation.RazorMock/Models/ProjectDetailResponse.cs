namespace TaskStatusTransitionValidation.RazorMock.Models;

public sealed class ProjectDetailResponse
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
}