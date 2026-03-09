namespace TaskStatusTransitionValidation.RazorMock.Models
{
    public sealed class ProjectResponse
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
    }
}
