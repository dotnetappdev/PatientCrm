using Microsoft.AspNetCore.Authentication.Cookies;
using PatientCrm.Web.Components;
using PatientCrm.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor Web App with Interactive Server Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MVC – kept solely for the AccountController (Login / Logout / 2FA / PasskeyCallback)
// These actions need to call HttpContext.SignInAsync which requires the HTTP pipeline.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// Cookie authentication (web layer – JWT stays in the API)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// HttpContext accessor (used only during SSR in AccountController / App.razor)
builder.Services.AddHttpContextAccessor();

// Scoped token provider – captures the JWT from the cookie claim during SSR
// and makes it available throughout the Blazor Server circuit.
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<NotificationService>();

// Session (partial 2FA state)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromMinutes(10);
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;
});

// API client – all data access goes through the PatientCRM REST API
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001/";
builder.Services.AddHttpClient("PatientApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
// Register PatientApiClient as scoped so it can depend on the scoped TokenProvider
builder.Services.AddScoped<PatientApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var http = factory.CreateClient("PatientApi");
    var tokenProvider = sp.GetRequiredService<TokenProvider>();
    return new PatientApiClient(http, tokenProvider);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// MVC routes – handles /Account/* only
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Blazor routes – handles everything else
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
