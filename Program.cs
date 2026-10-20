using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using InvestEasy.Components;
using InvestEasy.Components.Account;
using InvestEasy.Data;
using InvestEasy.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registers a named HttpClient for Finnhub using BaseUrl from appsettings.
builder.Services.AddHttpClient("FinnhubClient", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["Finnhub:BaseUrl"] ?? "https://finnhub.io/api/v1/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Service registers.
builder.Services.AddScoped<MarketService>();
builder.Services.AddScoped<WatchListService>();

builder.Services.AddScoped<ICalculatorService, CalculatorService>();
builder.Services.AddScoped<SavingScenarioService>();

builder.Services.AddScoped<UserProgressService>();

builder.Services.AddScoped<CurrentUserService>();

builder.Services.AddScoped<RiskProfileService>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// SQLite creates the database file but not its folder, ex. "Data/" on a fresh server.
var databaseFolder = Path.GetDirectoryName(Path.GetFullPath(new SqliteConnectionStringBuilder(connectionString).DataSource, builder.Environment.ContentRootPath));
if (!string.IsNullOrEmpty(databaseFolder))
{
    Directory.CreateDirectory(databaseFolder);

    // Login cookies are encrypted with data protection keys. They are stored next to the database,
    // so users stay logged in after a restart or redeploy as long as that folder is kept.
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(databaseFolder, "keys")))
        .SetApplicationName("InvestEasy");
}

// Interactive pages live for the whole circuit, so services create a short-lived DbContext per operation
// through the factory. The factory also registers ApplicationDbContext as scoped, which Identity uses.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // There is no email sender, so accounts can't be confirmed by email.
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;

        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Most hosts run the app behind a reverse proxy that terminates HTTPS.
// The forwarded headers tell the app the original scheme, so redirects and secure cookies work.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Simple endpoint that hosting platforms can call to check that the app is running.
builder.Services.AddHealthChecks();

var app = builder.Build();

// Creates the database and applies new migrations on startup, so a fresh server only needs the app.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (string.IsNullOrWhiteSpace(app.Configuration["Finnhub:ApiKey"]))
{
    app.Logger.LogWarning("Finnhub:ApiKey is not set. Market data will be unavailable.");
}

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

// Numbers and dates are formatted the Swedish way (amounts are in kr), no matter which culture the server has.
app.UseRequestLocalization("sv-SE");

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Basic security headers for every response.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapHealthChecks("/health");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
