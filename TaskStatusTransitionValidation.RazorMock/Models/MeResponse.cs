using TaskStatusTransitionValidation.RazorMock.Services;

public sealed class MeResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Role { get; set; }
}
