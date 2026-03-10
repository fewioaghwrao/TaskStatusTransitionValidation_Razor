using AngleSharp.Html.Parser;

namespace TaskStatusTransitionValidation.RazorMock.Tests.Integration;

internal static class AntiforgeryHelper
{
    public static async Task<(string action, string token)> GetFormInfoAsync(
        HttpClient client,
        string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();

        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html);

        var form = document.Forms.FirstOrDefault();
        if (form is null)
        {
            throw new InvalidOperationException("Form was not found.");
        }

        var tokenInput = form.QuerySelector("input[name='__RequestVerificationToken']");
        if (tokenInput is null)
        {
            throw new InvalidOperationException("Antiforgery token input was not found.");
        }

        var token = tokenInput.GetAttribute("value");
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Antiforgery token value was empty.");
        }

        return (form.Action, token);
    }
}