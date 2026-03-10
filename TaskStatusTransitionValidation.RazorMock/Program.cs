using TaskStatusTransitionValidation.RazorMock.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl Ç™ñ¢ê›íËÇ≈Ç∑ÅB"));
});

builder.Services.AddScoped<IMeProvider, ApiMeProvider>();
builder.Services.AddScoped<ITaskStore, InMemoryTaskStore>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Errors/{0}");

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.Run();

public partial class Program
{
}