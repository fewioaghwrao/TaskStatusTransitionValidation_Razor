using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskStatusTransitionValidation.RazorMock.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ErrorModel(ILogger<ErrorModel> logger) : PageModel
{
    private readonly ILogger<ErrorModel> _logger = logger;

    public string? RequestId { get; private set; }

    public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);

    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        _logger.LogError(
            "Unhandled error page displayed. RequestId: {RequestId}, Path: {Path}, Method: {Method}",
            RequestId,
            HttpContext.Request.Path,
            HttpContext.Request.Method);
    }
}
