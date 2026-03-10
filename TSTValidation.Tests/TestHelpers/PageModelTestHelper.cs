using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System.Net.Http;

namespace TaskStatusTransitionValidation.RazorMock.Tests.TestHelpers;

public static class PageModelTestHelper
{
    public static void SetHttpContext(PageModel pageModel, string? authToken = null)
    {
        var httpContext = new DefaultHttpContext();

        if (!string.IsNullOrWhiteSpace(authToken))
        {
            httpContext.Request.Headers.Cookie = $"auth_token={authToken}";
        }

        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData()
        };

        pageModel.TempData = new TempDataDictionary(
            httpContext,
            new SimpleTempDataProvider());
    }

    private sealed class SimpleTempDataProvider : ITempDataProvider
    {
        private Dictionary<string, object> _data = new();

        public IDictionary<string, object> LoadTempData(HttpContext context)
            => _data;

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
            => _data = new Dictionary<string, object>(values);
    }
}