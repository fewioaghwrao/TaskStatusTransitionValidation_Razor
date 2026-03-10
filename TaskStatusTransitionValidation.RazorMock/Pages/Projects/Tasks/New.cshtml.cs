using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Services;

namespace TaskStatusTransitionValidation.RazorMock.Pages.Projects.Tasks;

public class NewModel(IApiClient apiClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int ProjectId { get; set; }

    public MeResponse? Me { get; private set; }

    public IReadOnlyList<ProjectMemberDto> Members { get; private set; } = [];

    [BindProperty]
    public TaskCreateInputModel Input { get; set; } = new();

    [BindProperty]
    public bool ConfirmSubmit { get; set; }

    public string? ErrorMessage { get; private set; }

    public bool IsBusy { get; private set; }

    public bool IsLeader =>
        string.Equals(Me?.Role, "Leader", StringComparison.OrdinalIgnoreCase);

    public bool CanCreate =>
        Me is not null &&
        (IsLeader || Members.Any(m => m.UserId == Me.UserId));

    public async Task<IActionResult> OnGetAsync(int projectId, CancellationToken cancellationToken)
    {
        ProjectId = projectId;
        Input.ProjectId = projectId;

        var authResult = await LoadCurrentContextAsync(projectId, cancellationToken);
        if (authResult is not null)
        {
            return authResult;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int projectId, CancellationToken cancellationToken)
    {
        ProjectId = projectId;
        Input.ProjectId = projectId;

        var authResult = await LoadCurrentContextAsync(projectId, cancellationToken);
        if (authResult is not null)
        {
            return authResult;
        }

        NormalizeInput();

        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError("Input.Title", "タイトルは必須です。");
        }

        if (!CanCreate)
        {
            ErrorMessage = "この案件のメンバーではないため、タスクを作成できません。";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!ConfirmSubmit)
        {
            ErrorMessage = "確認ダイアログから作成を確定してください。";
            return Page();
        }

        var token = Request.Cookies["auth_token"]!;

        var request = new TaskCreateRequest
        {
            ProjectId = Input.ProjectId,
            Title = Input.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description.Trim(),
            DueDate = Input.DueDate,
            Priority = Input.Priority,
            AssigneeUserId = IsLeader
                ? Input.AssigneeUserId
                : null
        };

        IsBusy = true;

        var success = await apiClient.CreateTaskAsync(token, request, cancellationToken);

        if (!success)
        {
            ErrorMessage = "タスク作成に失敗しました。";
            IsBusy = false;
            return Page();
        }

        return RedirectToPage("/Projects/Tasks/Index", new { projectId = ProjectId });
    }

    private async Task<IActionResult?> LoadCurrentContextAsync(int projectId, CancellationToken cancellationToken)
    {
        IsBusy = true;

        var token = Request.Cookies["auth_token"];

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Login");
        }

        var me = await apiClient.GetMeAsync(token, cancellationToken);
        if (me is null)
        {
            return RedirectToPage("/Login");
        }

        Me = me;

        Members = await apiClient.GetProjectMembersAsync(token, projectId, cancellationToken)
                  ?? [];

        IsBusy = false;
        return null;
    }

    private void NormalizeInput()
    {
        Input.Title = Input.Title?.Trim() ?? string.Empty;
        Input.Description = string.IsNullOrWhiteSpace(Input.Description)
            ? null
            : Input.Description.Trim();

        if (!IsLeader)
        {
            Input.AssigneeUserId = null;
        }
    }

    public sealed class TaskCreateInputModel
    {
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "タイトルは必須です。")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DueDate { get; set; }

        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public int? AssigneeUserId { get; set; }
    }
}