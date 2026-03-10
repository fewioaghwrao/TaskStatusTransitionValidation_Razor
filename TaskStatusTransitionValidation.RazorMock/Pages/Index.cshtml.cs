using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskStatusTransitionValidation.RazorMock.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    public IActionResult OnGet()
    {
        var token = Request.Cookies["auth_token"];

        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogInformation("Token not found. Redirecting to Login.");
            return RedirectToPage("/Login");
        }

        logger.LogInformation("Token found. Redirecting to Projects.");
        return RedirectToPage("/Projects/Index");
    }
}