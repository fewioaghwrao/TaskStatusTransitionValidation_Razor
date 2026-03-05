using TaskStatusTransitionValidation.RazorMock.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddSingleton<ITaskStore, MockTaskStore>();
builder.Services.AddSingleton<IMeProvider, MockMeProvider>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.Run();
