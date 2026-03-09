using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Services;

namespace TaskStatusTransitionValidation.RazorMock.Pages.Projects.Tasks;

public class NewModel(ApiClient apiClient, IMeProvider meProvider) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int ProjectId { get; set; }

    public MeResponse? Me { get; private set; }

    public IReadOnlyList<ProjectMemberDto> Members { get; private set; } = [];

    [BindProperty]
    public TaskCreateRequest Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public bool IsLeader =>
        string.Equals(Me?.Role, "Leader", StringComparison.OrdinalIgnoreCase);

    public async Task<IActionResult> OnGetAsync(int projectId, CancellationToken cancellationToken)
    {
        ProjectId = projectId;

        var token = Request.Cookies["auth_token"];

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Login");
        }

        var me = await apiClient.GetMeAsync(token, cancellationToken);

        if (me == null)
        {
            return RedirectToPage("/Login");
        }

        Me = me;

        Members = await apiClient.GetProjectMembersAsync(token, projectId, cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var token = Request.Cookies["auth_token"];

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var success = await apiClient.CreateTaskAsync(token, Input, cancellationToken);

        if (!success)
        {
            ErrorMessage = "タスク作成に失敗しました。";
            return Page();
        }

        return RedirectToPage("/Projects/Index");
    }
}