using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Services;

namespace TaskStatusTransitionValidation.RazorMock.Pages.Projects.Tasks;

public class TaskDetailModel(IApiClient apiClient) : PageModel
{
    private static readonly string[] AllowedStatuses = ["ToDo", "Doing", "Blocked", "Done"];
    private static readonly string[] AllowedPriorities = ["High", "Medium", "Low"];

    [BindProperty(SupportsGet = true)]
    public int ProjectId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int TaskId { get; set; }

    public MeResponse? Me { get; private set; }

    public IReadOnlyList<ProjectMemberDto> Members { get; private set; } = [];

    public TaskResponse? OriginalTask { get; private set; }

    [BindProperty]
    public TaskEditInputModel Input { get; set; } = new();

    [BindProperty]
    public bool ConfirmSubmit { get; set; }

    public string? ErrorMessage { get; private set; }

    public bool IsBusy { get; private set; }

    public bool IsDone =>
        string.Equals(OriginalTask?.Status, "Done", StringComparison.OrdinalIgnoreCase);

    public async Task<IActionResult> OnGetAsync(int projectId, int taskId, CancellationToken cancellationToken)
    {
        ProjectId = projectId;
        TaskId = taskId;

        var loadResult = await LoadCurrentContextAsync(projectId, taskId, cancellationToken);
        if (loadResult is not null)
        {
            return loadResult;
        }

        if (OriginalTask is null)
        {
            ErrorMessage = "タスク情報の取得に失敗しました。";
            return Page();
        }

        MapTaskToInput(OriginalTask);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int projectId, int taskId, CancellationToken cancellationToken)
    {
        ProjectId = projectId;
        TaskId = taskId;

        var loadResult = await LoadCurrentContextAsync(projectId, taskId, cancellationToken);
        if (loadResult is not null)
        {
            return loadResult;
        }

        if (OriginalTask is null)
        {
            ErrorMessage = "タスク情報の取得に失敗しました。";
            return Page();
        }

        NormalizeInput();

        ValidateInput();

        if (IsDone)
        {
            ErrorMessage = "完了（Done）のタスクは更新できません。";
            return Page();
        }

        if (!CanTransition(OriginalTask.Status, Input.Status))
        {
            ErrorMessage = $"状態遷移が許可されていません: {OriginalTask.Status} → {Input.Status}";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!ConfirmSubmit)
        {
            ErrorMessage = "確認ダイアログから更新を確定してください。";
            return Page();
        }

        var token = Request.Cookies["auth_token"]!;

        var request = new TaskUpdateRequest
        {
            Title = Input.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description.Trim(),
            DueDate = string.IsNullOrWhiteSpace(Input.DueDate) ? null : Input.DueDate,
            Priority = Input.Priority,
            Status = Input.Status,
            AssigneeUserId = OriginalTask.AssigneeUserId
        };

        IsBusy = true;

        var success = await apiClient.UpdateTaskAsync(token, taskId, request, cancellationToken);

        if (!success)
        {
            ErrorMessage = "タスク更新に失敗しました。";
            IsBusy = false;
            return Page();
        }

        return RedirectToPage("/Projects/Tasks/Index", new { projectId = ProjectId });
    }

    private async Task<IActionResult?> LoadCurrentContextAsync(int projectId, int taskId, CancellationToken cancellationToken)
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

        Members = await apiClient.GetProjectMembersAsync(token, projectId, cancellationToken) ?? [];

        OriginalTask = await apiClient.GetTaskByIdAsync(token, taskId, cancellationToken);

        IsBusy = false;
        return null;
    }

    private void MapTaskToInput(TaskResponse task)
    {
        Input = new TaskEditInputModel
        {
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Priority = task.Priority,
            Status = task.Status
        };
    }

    private void NormalizeInput()
    {
        Input.Title = Input.Title?.Trim() ?? string.Empty;
        Input.Description = string.IsNullOrWhiteSpace(Input.Description)
            ? null
            : Input.Description.Trim();
        Input.DueDate = string.IsNullOrWhiteSpace(Input.DueDate)
            ? null
            : Input.DueDate.Trim();
        Input.Priority = Input.Priority?.Trim() ?? string.Empty;
        Input.Status = Input.Status?.Trim() ?? string.Empty;
    }

    private void ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError("Input.Title", "タイトルは必須です。");
        }

        if (!AllowedPriorities.Contains(Input.Priority, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Input.Priority", "優先度が不正です。");
        }

        if (!AllowedStatuses.Contains(Input.Status, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Input.Status", "状態が不正です。");
        }

        if (!string.IsNullOrWhiteSpace(Input.DueDate) &&
            !DateOnly.TryParse(Input.DueDate, out _))
        {
            ModelState.AddModelError("Input.DueDate", "期限は yyyy-MM-dd 形式で入力してください。");
        }
    }

    public string AssigneeLabel(int? userId)
    {
        if (userId is null)
        {
            return "未割当";
        }

        var member = Members.FirstOrDefault(x => x.UserId == userId.Value);
        if (member is null)
        {
            return $"ユーザー#{userId.Value}";
        }

        if (!string.IsNullOrWhiteSpace(member.DisplayName))
        {
            return $"{member.DisplayName}（{member.Email}）";
        }

        return member.Email;
    }

    public string StatusLabelJa(string? status)
    {
        return status switch
        {
            "ToDo" => "未着手",
            "Doing" => "作業中",
            "Blocked" => "ブロック中",
            "Done" => "完了",
            _ => status ?? "-"
        };
    }

    public bool CanSelectStatus(string nextStatus)
    {
        if (OriginalTask is null)
        {
            return false;
        }

        return CanTransition(OriginalTask.Status, nextStatus);
    }

    private static bool CanTransition(string? from, string? to)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            return false;

        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(from, "Done", StringComparison.OrdinalIgnoreCase))
            return false;

        if (string.Equals(from, "ToDo", StringComparison.OrdinalIgnoreCase))
            return string.Equals(to, "Doing", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(from, "Doing", StringComparison.OrdinalIgnoreCase))
            return string.Equals(to, "Done", StringComparison.OrdinalIgnoreCase)
                || string.Equals(to, "Blocked", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(from, "Blocked", StringComparison.OrdinalIgnoreCase))
            return string.Equals(to, "Doing", StringComparison.OrdinalIgnoreCase);

        return false;
    }

    public sealed class TaskEditInputModel
    {
        [Required(ErrorMessage = "タイトルは必須です。")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? DueDate { get; set; }

        [Required(ErrorMessage = "優先度は必須です。")]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "状態は必須です。")]
        public string Status { get; set; } = "ToDo";
    }
}