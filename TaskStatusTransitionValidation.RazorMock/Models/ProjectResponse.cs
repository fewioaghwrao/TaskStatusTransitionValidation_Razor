namespace TaskStatusTransitionValidation.RazorMock.Models
{
    public sealed class ProjectResponse
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
    }
}
