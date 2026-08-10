using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Outfitters.Web.Authentication;
using Outfitters.Web.Components;
using Outfitters.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
builder.Services.AddScoped<AuthSession>();
builder.Services.AddScoped<BearerTokenHandler>();
builder.Services.AddScoped<AuthenticationStateProvider, OrmsAuthenticationStateProvider>();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:8080";

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<BearerTokenHandler>();

builder.Services.AddHttpClient<IRetailApiClient, RetailApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<BearerTokenHandler>();

builder.Services
    .AddHttpClient<IRetailOperationsApiClient, RetailOperationsApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<BearerTokenHandler>();

builder.Services
    .AddHttpClient<IInventoryIntelligenceApiClient, InventoryIntelligenceApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<BearerTokenHandler>();

builder.Services
    .AddHttpClient<ICrmApiClient, CrmApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<BearerTokenHandler>();

builder.Services
    .AddHttpClient<IPurchasingApiClient, PurchasingApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<BearerTokenHandler>();

builder.Services
    .AddHttpClient<IHrApiClient, HrApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<BearerTokenHandler>();

var app = builder.Build();
var authenticationService =
    app.Services.GetService<Microsoft.AspNetCore.Authentication.IAuthenticationService>();

Console.WriteLine(
    authenticationService is null
        ? "DI CHECK: IAuthenticationService = MISSING"
        : "DI CHECK: IAuthenticationService = REGISTERED");
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Outfitters.Web", UtcTime = DateTime.UtcNow }));
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
