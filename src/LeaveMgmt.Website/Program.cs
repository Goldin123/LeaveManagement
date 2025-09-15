using LeaveMgmt.Website.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register HttpClientFactory
builder.Services.AddHttpClient();  // <-- ensures IHttpClientFactory is available

// Named client for your API
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
                                 ?? "https://localhost:7186/");
});

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProtectedLocalStorage>();

var app = builder.Build();

// usual pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
