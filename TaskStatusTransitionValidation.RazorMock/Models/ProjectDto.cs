namespace TaskStatusTransitionValidation.RazorMock.Models;

public sealed class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
}