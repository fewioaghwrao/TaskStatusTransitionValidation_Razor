using TaskStatusTransitionValidation.RazorMock.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl ‚ª–¢İ’è‚Å‚·B"));
});

builder.Services.AddScoped<IMeProvider, ApiMeProvider>();
builder.Services.AddScoped<ITaskStore, InMemoryTaskStore>(); // ‚±‚ê‚ÍŠù‘¶‚ÌÀ‘•‚É‡‚í‚¹‚é

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.Run();