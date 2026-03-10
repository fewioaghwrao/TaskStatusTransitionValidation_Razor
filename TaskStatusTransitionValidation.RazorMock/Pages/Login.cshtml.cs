using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskStatusTransitionValidation.RazorMock.Services;

public class LoginModel : PageModel
{
    private readonly IApiClient _api;

    public LoginModel(IApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public LoginRequest Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _api.LoginAsync(Input);

        if (result == null)
        {
            ErrorMessage = "ログインに失敗しました。入力内容をご確認ください。";
            return Page();
        }

        Response.Cookies.Append("auth_token", result.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // HTTPS化したら true 推奨
            SameSite = SameSiteMode.Lax
        });

        return RedirectToPage("/Projects/Index");
    }
}