using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskStatusTransitionValidation.RazorMock.Models;
using TaskStatusTransitionValidation.RazorMock.Services;

namespace TaskStatusTransitionValidation.RazorMock.Pages.Projects;

public class NewModel(IApiClient apiClient) : PageModel
{
    public MeResponse? Me { get; private set; }

    [BindProperty]
    public ProjectCreateRequest Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public bool IsLeader =>
        string.Equals(Me?.Role, "Leader", StringComparison.OrdinalIgnoreCase);

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var token = Request.Cookies["auth_token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Login");
        }

        var me = await apiClient.GetMeAsync(token, cancellationToken);
        if (me is null)
        {
            Response.Cookies.Delete("auth_token");
            return RedirectToPage("/Login");
        }

        Me = me;

        if (!IsLeader)
        {
            return RedirectToPage("/Projects/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var token = Request.Cookies["auth_token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Login");
        }

        Me = await apiClient.GetMeAsync(token, cancellationToken);
        if (Me is null)
        {
            Response.Cookies.Delete("auth_token");
            return RedirectToPage("/Login");
        }

        var name = (Input.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorMessage = "プロジェクト名を入力してください。";
            return Page();
        }

        if (!IsLeader)
        {
            ErrorMessage = "この操作はリーダーのみ可能です。";
            return Page();
        }

        Input.Name = name;

        var created = await apiClient.CreateProjectAsync(token, Input, cancellationToken);
        if (created is null)
        {
            ErrorMessage = "案件の作成に失敗しました。";
            return Page();
        }

        return RedirectToPage("/Projects/Index");
    }
}