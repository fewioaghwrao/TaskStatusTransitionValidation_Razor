using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Services;
using System.Text;
using System.IO;

namespace TaskStatusTransitionValidation.RazorMock.Pages.Projects.Tasks;

public class IndexModel(IApiClient apiClient, IMeProvider meProvider) : PageModel
{
    private const int DueSoonDays = 7;

    public int ProjectId { get; private set; }

    public MeDto? Me { get; private set; }
    public ProjectDetailResponse? Project { get; private set; }

    public IReadOnlyList<TaskResponse> AllTasks { get; private set; } = [];
    public IReadOnlyList<TaskResponse> FilteredTasks { get; private set; } = [];
    public IReadOnlyList<TaskResponse> PageItems { get; private set; } = [];
    public IReadOnlyList<ProjectMemberDto> Members { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public string StatusF { get; set; } = "All";

    [BindProperty(SupportsGet = true)]
    public string PrioF { get; set; } = "All";

    [BindProperty(SupportsGet = true)]
    public string DueF { get; set; } = "All";

    [BindProperty(SupportsGet = true)]
    public int PageNo { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public int TotalFiltered { get; private set; }
    public int TotalPages { get; private set; }
    public int SafePage { get; private set; }

    public int DisplayStart => TotalFiltered == 0 ? 0 : ((SafePage - 1) * PageSize) + 1;
    public int DisplayEnd => TotalFiltered == 0 ? 0 : Math.Min(SafePage * PageSize, TotalFiltered);

    public int OverdueCount { get; private set; }
    public int DueSoonCount { get; private set; }
    public int ToDoCount { get; private set; }
    public int DoingCount { get; private set; }
    public int BlockedCount { get; private set; }
    public int DoneCount { get; private set; }

    public string Title => !string.IsNullOrWhiteSpace(Project?.Name) ? Project!.Name : $"案件 {ProjectId}";

    public bool IsLeader => Me?.Role == UserRole.Leader;

    public bool IsMember =>
        Me is not null &&
        Members.Any(x => x.UserId == Me.Id);

    public bool CanCreateTask => IsLeader || IsMember;

    public IReadOnlyList<string> AllStatuses { get; } = ["ToDo", "Doing", "Blocked", "Done"];

    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? OperationMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int projectId, CancellationToken cancellationToken)
    {
        ProjectId = projectId;
        return await LoadPageAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(
        int projectId,
        int taskId,
        string nextStatus,
        string? q,
        string? statusF,
        string? prioF,
        string? dueF,
        int pageNo,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ProjectId = projectId;
        Q = q;
        StatusF = statusF ?? "All";
        PrioF = prioF ?? "All";
        DueF = dueF ?? "All";
        PageNo = pageNo <= 0 ? 1 : pageNo;
        PageSize = NormalizePageSize(pageSize);

        var token = Request.Cookies["auth_token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Login");
        }

        try
        {
            var meResponse = await meProvider.GetMeAsync(token, cancellationToken);
            if (meResponse is null)
            {
                Response.Cookies.Delete("auth_token");
                return RedirectToPage("/Login");
            }

            var tasks = await apiClient.GetTasksByProjectIdAsync(token, projectId, cancellationToken);
            var task = tasks.FirstOrDefault(x => x.TaskId == taskId);

            if (task is null)
            {
                ErrorMessage = "対象タスクが見つかりません。";
                return RedirectToCurrentPage();
            }

            if (!CanTransition(task.Status, nextStatus))
            {
                ErrorMessage = $"状態遷移が許可されていません: {task.Status} → {nextStatus}";
                return RedirectToCurrentPage();
            }

            var updated = await apiClient.UpdateTaskAsync(token, taskId, new TaskUpdateRequest
            {
                Title = task.Title,
                Description = task.Description,
                AssigneeUserId = task.AssigneeUserId,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = nextStatus
            }, cancellationToken);

            if (!updated)
            {
                ErrorMessage = "タスクの状態更新に失敗しました。";
                return RedirectToCurrentPage();
            }

            OperationMessage = $"タスク #{taskId} の状態を {StatusLabelJa(nextStatus)} に更新しました。";
            return RedirectToCurrentPage();
        }
        catch
        {
            ErrorMessage = "タスクの状態更新中にエラーが発生しました。";
            return RedirectToCurrentPage();
        }
    }

    public async Task<IActionResult> OnPostArchiveProjectAsync(int projectId, CancellationToken cancellationToken)
    {
        ProjectId = projectId;

        var token = Request.Cookies["auth_token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Login");
        }

        try
        {
            var archived = await apiClient.ArchiveProjectAsync(token, projectId, cancellationToken);

            if (!archived)
            {
                ErrorMessage = "案件のアーカイブに失敗しました。";
                return RedirectToPage("/Projects/Tasks/Index", new { projectId });
            }

            OperationMessage = "案件をアーカイブしました。";
            return RedirectToPage("/Projects/Index");
        }
        catch
        {
            ErrorMessage = "案件のアーカイブ中にエラーが発生しました。";
            return RedirectToPage("/Projects/Tasks/Index", new { projectId });
        }
    }

    public async Task<IActionResult> OnPostExportCsvAsync(
    int projectId,
    string? q,
    string? statusF,
    string? prioF,
    string? dueF,
    int pageNo,
    int pageSize,
    CancellationToken cancellationToken)
    {
        ProjectId = projectId;
        Q = q;
        StatusF = statusF ?? "All";
        PrioF = prioF ?? "All";
        DueF = dueF ?? "All";
        PageNo = pageNo <= 0 ? 1 : pageNo;
        PageSize = NormalizePageSize(pageSize);

        var token = Request.Cookies["auth_token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Login");
        }

        try
        {
            var meResponse = await meProvider.GetMeAsync(token, cancellationToken);
            if (meResponse is null)
            {
                Response.Cookies.Delete("auth_token");
                return RedirectToPage("/Login");
            }

            Project = await apiClient.GetProjectByIdAsync(token, ProjectId, cancellationToken);
            Members = await apiClient.GetProjectMembersAsync(token, ProjectId, cancellationToken);
            AllTasks = await apiClient.GetTasksByProjectIdAsync(token, ProjectId, cancellationToken);

            var filtered = ApplyFilters(AllTasks, Q, StatusF, PrioF, DueF);

            var csv = BuildTasksCsvRows(filtered, Members);
            var fileBaseName = SanitizeFileName($"{Title}_tasks_{DateTime.Today:yyyy-MM-dd}");
            var fileName = $"{fileBaseName}.csv";

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
        catch
        {
            ErrorMessage = "CSV出力に失敗しました。";
            return RedirectToCurrentPage();
        }
    }

    private async Task<IActionResult> LoadPageAsync(CancellationToken cancellationToken)
    {
        try
        {
            var token = Request.Cookies["auth_token"];
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Login");
            }

            var meResponse = await meProvider.GetMeAsync(token, cancellationToken);
            if (meResponse is null)
            {
                Response.Cookies.Delete("auth_token");
                return RedirectToPage("/Login");
            }

            Me = new MeDto
            {
                Id = meResponse.UserId,
                Name = meResponse.DisplayName ?? string.Empty,
                DisplayName = meResponse.DisplayName ?? string.Empty,
                Email = meResponse.Email ?? string.Empty,
                Role = string.Equals(meResponse.Role, "Leader", StringComparison.OrdinalIgnoreCase)
                    ? UserRole.Leader
                    : UserRole.Worker
            };

            Project = await apiClient.GetProjectByIdAsync(token, ProjectId, cancellationToken);
            Members = await apiClient.GetProjectMembersAsync(token, ProjectId, cancellationToken);
            AllTasks = await apiClient.GetTasksByProjectIdAsync(token, ProjectId, cancellationToken);

            AllTasks = AllTasks
                .OrderByDescending(x => x.TaskId)
                .ToList();

            BuildSummary(AllTasks);

            var keyword = (Q ?? string.Empty).Trim();

            var filtered = ApplyFilters(AllTasks, Q, StatusF, PrioF, DueF);

            FilteredTasks = filtered;
            TotalFiltered = filtered.Count;

            PageSize = NormalizePageSize(PageSize);
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalFiltered / (double)PageSize));
            SafePage = Math.Min(Math.Max(1, PageNo), TotalPages);

            PageItems = filtered
                .Skip((SafePage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }
        catch
        {
            Response.Cookies.Delete("auth_token");
            return RedirectToPage("/Login");
        }
    }

    private IActionResult RedirectToCurrentPage()
    {
        return RedirectToPage("/Projects/Tasks/Index", new
        {
            projectId = ProjectId,
            q = Q,
            statusF = StatusF,
            prioF = PrioF,
            dueF = DueF,
            pageNo = PageNo,
            pageSize = PageSize
        });
    }

    private void BuildSummary(IEnumerable<TaskResponse> tasks)
    {
        OverdueCount = 0;
        DueSoonCount = 0;
        ToDoCount = 0;
        DoingCount = 0;
        BlockedCount = 0;
        DoneCount = 0;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var dueSoon = today.AddDays(DueSoonDays);

        foreach (var task in tasks)
        {
            if (string.Equals(task.Status, "ToDo", StringComparison.OrdinalIgnoreCase))
            {
                ToDoCount++;
            }
            else if (string.Equals(task.Status, "Doing", StringComparison.OrdinalIgnoreCase))
            {
                DoingCount++;
            }
            else if (string.Equals(task.Status, "Blocked", StringComparison.OrdinalIgnoreCase))
            {
                BlockedCount++;
            }
            else if (string.Equals(task.Status, "Done", StringComparison.OrdinalIgnoreCase))
            {
                DoneCount++;
            }

            if (string.Equals(task.Status, "Done", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var due = ToDateOnly(task.DueDate);
            if (due is null)
            {
                continue;
            }

            if (due.Value < today)
            {
                OverdueCount++;
            }
            else if (due.Value >= today && due.Value <= dueSoon)
            {
                DueSoonCount++;
            }
        }
    }

    private static int NormalizePageSize(int pageSize)
    {
        return pageSize switch
        {
            5 => 5,
            10 => 10,
            20 => 20,
            _ => 10
        };
    }

    private static DateOnly? ToDateOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var s = value.Length >= 10 ? value[..10] : value;

        return DateOnly.TryParse(s, out var date) ? date : null;
    }

    public string FormatDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        return value.Length >= 10 ? value[..10] : value;
    }

    public bool IsTaskFixed(string? status)
    {
        return string.Equals(status, "Done", StringComparison.OrdinalIgnoreCase);
    }

    public bool CanTransition(string? from, string? to)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
        {
            return false;
        }

        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(from, "Done", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(from, "ToDo", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(to, "Doing", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(from, "Doing", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(to, "Done", StringComparison.OrdinalIgnoreCase)
                || string.Equals(to, "Blocked", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(from, "Blocked", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(to, "Doing", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public string StatusLabelJa(string? status)
    {
        return status switch
        {
            "ToDo" => "未着手",
            "Doing" => "作業中",
            "Blocked" => "ブロック中",
            "Done" => "完了",
            _ => status ?? string.Empty
        };
    }

    public string GetPriorityCssClass(string? priority)
    {
        return priority switch
        {
            "High" => "pill-priority-high",
            "Medium" => "pill-priority-medium",
            "Low" => "pill-priority-low",
            _ => string.Empty
        };
    }

    public string GetStatusCssClass(string? status)
    {
        return status switch
        {
            "ToDo" => "pill-status-todo",
            "Doing" => "pill-status-doing",
            "Blocked" => "pill-status-blocked",
            "Done" => "pill-status-done",
            _ => string.Empty
        };
    }

    private List<TaskResponse> ApplyFilters(
    IEnumerable<TaskResponse> tasks,
    string? q,
    string? statusF,
    string? prioF,
    string? dueF)
    {
        var keyword = (q ?? string.Empty).Trim();

        return tasks.Where(t =>
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var hitTitle = t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase);
                var hitDesc = t.Description?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false;
                if (!hitTitle && !hitDesc)
                {
                    return false;
                }
            }

            if (!string.Equals(statusF, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(t.Status, statusF, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.Equals(prioF, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(t.Priority, prioF, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.Equals(dueF, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(t.Status, "Done", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var due = ToDateOnly(t.DueDate);
                if (due is null)
                {
                    return false;
                }

                var today = DateOnly.FromDateTime(DateTime.Today);
                var dueSoon = today.AddDays(DueSoonDays);

                if (string.Equals(dueF, "Overdue", StringComparison.OrdinalIgnoreCase))
                {
                    if (!(due.Value < today))
                    {
                        return false;
                    }
                }
                else if (string.Equals(dueF, "DueSoon", StringComparison.OrdinalIgnoreCase))
                {
                    if (!(due.Value >= today && due.Value <= dueSoon))
                    {
                        return false;
                    }
                }
            }

            return true;
        }).ToList();
    }

    private string BuildTasksCsvRows(
    IReadOnlyList<TaskResponse> tasks,
    IReadOnlyList<ProjectMemberDto> members)
    {
        var memberMap = members.ToDictionary(x => x.UserId, x => x);

        var lines = new List<string>();

        var header = new[]
        {
        "TaskId",
        "Title",
        "Description",
        "StatusJa",
        "Status",
        "Priority",
        "DueDate",
        "AssigneeDisplayName"
    };

        lines.Add(string.Join(",", header.Select(CsvEscape)));

        foreach (var task in tasks)
        {
            var assigneeDisplayName = string.Empty;

            if (task.AssigneeUserId is not null)
            {
                assigneeDisplayName = memberMap.TryGetValue(task.AssigneeUserId.Value, out var member)
                    ? member.DisplayName
                    : $"User#{task.AssigneeUserId.Value}";
            }

            var row = new[]
            {
            task.TaskId.ToString(),
            task.Title,
            task.Description ?? string.Empty,
            StatusLabelJa(task.Status),
            task.Status,
            task.Priority,
            FormatDateForCsv(task.DueDate),
            assigneeDisplayName
        };

            lines.Add(string.Join(",", row.Select(CsvEscape)));
        }

        return string.Join("\r\n", lines);
    }

    private string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var escaped = value.Replace("\"", "\"\"");
        var needsQuote = escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\r') || escaped.Contains('\n');

        return needsQuote ? $"\"{escaped}\"" : escaped;
    }

    private string FormatDateForCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length >= 10 ? value[..10] : value;
    }

    public string SanitizeFileName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "tasks";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());

        return sanitized.Trim();
    }
}