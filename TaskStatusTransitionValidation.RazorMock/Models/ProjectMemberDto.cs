namespace TaskStatusTransitionValidation.RazorMock.Models;

public sealed class ProjectMemberDto
{
    public int UserId { get; set; }
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
}