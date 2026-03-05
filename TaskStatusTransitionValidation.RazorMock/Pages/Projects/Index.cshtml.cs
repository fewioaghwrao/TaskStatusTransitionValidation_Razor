using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskStatusTransitionValidation.RazorMock.Services;

namespace TaskStatusTransitionValidation.RazorMock.Pages.Projects;

public class IndexModel(ITaskStore store, IMeProvider meProvider) : PageModel
{
    public MeDto Me { get; private set; } = default!;
    public IReadOnlyList<ProjectDto> Projects { get; private set; } = [];

    public void OnGet()
    {
        Me = meProvider.GetMe();
        Projects = store.GetProjects();
    }
}