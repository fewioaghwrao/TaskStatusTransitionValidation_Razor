using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskStatusTransitionValidation.RazorMock.Services;

namespace TaskStatusTransitionValidation.RazorMock.Pages;

public class LoginModel(IMeProvider meProvider) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        public string DisplayName { get; set; } = "Mock User";
        public UserRole Role { get; set; } = UserRole.Leader;
    }

    public void OnGet()
    {
        var me = meProvider.GetMe();
        Input.DisplayName = me.DisplayName;
        Input.Role = me.Role;
    }

    public IActionResult OnPost()
    {
        meProvider.SetMe(new MeDto(Input.DisplayName, Input.Role));
        return RedirectToPage("/Projects/Index");
    }
}
