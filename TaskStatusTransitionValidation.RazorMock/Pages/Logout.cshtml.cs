using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskStatusTransitionValidation.RazorMock.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnPost()
    {
        Response.Cookies.Delete("auth_token");

        return RedirectToPage("/Login");
    }
}