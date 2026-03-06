using TaskStatusTransitionValidation.RazorMock.Services;

namespace TaskStatusTransitionValidation.RazorMock.Models;

public sealed class MeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Worker;
}