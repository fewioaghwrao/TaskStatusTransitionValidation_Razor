using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Services;

namespace TaskStatusTransitionValidation.RazorMock.Pages.Projects;

public class IndexModel(ApiClient apiClient, IMeProvider meProvider) : PageModel
{
    public MeDto Me { get; private set; } = new();

    public IReadOnlyList<ProjectDto> AllProjects { get; private set; } = [];
    public IReadOnlyList<ProjectDto> ActivePageItems { get; private set; } = [];
    public IReadOnlyList<ProjectDto> ArchivedProjects { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 5;

    public int ActiveCount { get; private set; }
    public int ArchivedCount { get; private set; }
    public int TotalActiveFiltered { get; private set; }
    public int TotalPages { get; private set; }
    public int SafePage { get; private set; }

    public bool IsLeader => Me.Role == UserRole.Leader;

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var token = Request.Cookies["auth_token"];

            if (string.IsNullOrWhiteSpace(token))
            {
                SetErrorState("認証トークンが見つかりません。先にログインしてください。");
                return;
            }

            var meResponse = await meProvider.GetMeAsync(token, cancellationToken);
            if (meResponse is null)
            {
                SetErrorState("ユーザー情報の取得に失敗しました。");
                return;
            }

            Me = new MeDto
            {
                Id =0,
                Name = meResponse.DisplayName ?? string.Empty,
                DisplayName = meResponse.DisplayName ?? string.Empty,
                Email = meResponse.Email ?? string.Empty,
                Role = meResponse.Role == UserRole.Leader
                    ? UserRole.Leader
                    : UserRole.Worker
            };

            var projects = await apiClient.GetProjectsAsync(token, cancellationToken);

            AllProjects = projects
                .Select(x => new ProjectDto
                {
                    Id = x.ProjectId,
                    Name = x.Name,
                    Description = x.Description,
                    IsArchived = x.IsArchived
                })
                .ToList();

            var keyword = (Q ?? string.Empty).Trim();

            var filtered = string.IsNullOrWhiteSpace(keyword)
                ? AllProjects.ToList()
                : AllProjects
                    .Where(x =>
                        x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        (x.Description?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();

            var activeProjects = filtered.Where(x => !x.IsArchived).ToList();
            var archivedProjects = filtered.Where(x => x.IsArchived).ToList();

            ActiveCount = AllProjects.Count(x => !x.IsArchived);
            ArchivedCount = AllProjects.Count(x => x.IsArchived);

            TotalActiveFiltered = activeProjects.Count;

            PageSize = NormalizePageSize(PageSize);
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalActiveFiltered / (double)PageSize));
            SafePage = Math.Min(Math.Max(1, Page), TotalPages);

            ActivePageItems = activeProjects
                .Skip((SafePage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ArchivedProjects = archivedProjects;
        }
        catch (Exception ex)
        {
            SetErrorState(ex.Message);
        }
    }

    private void SetErrorState(string message)
    {
        ErrorMessage = message;

        Me = new MeDto
        {
            Id = 0,
            Name = "Unknown",
            DisplayName = "Unknown",
            Email = string.Empty,
            Role = UserRole.Worker
        };

        AllProjects = [];
        ActivePageItems = [];
        ArchivedProjects = [];
        ActiveCount = 0;
        ArchivedCount = 0;
        TotalActiveFiltered = 0;
        TotalPages = 1;
        SafePage = 1;
    }

    private static int NormalizePageSize(int pageSize)
    {
        return pageSize switch
        {
            5 => 5,
            10 => 10,
            20 => 20,
            _ => 5
        };
    }
}