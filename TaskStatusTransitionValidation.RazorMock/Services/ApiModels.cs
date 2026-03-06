namespace TaskStatusTransitionValidation.RazorMock.Services;

public enum UserRole
{
    Leader,
    Worker
}

public sealed class MeResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public UserRole Role { get; set; }
}
