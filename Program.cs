using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using InvestEasy.Components;
using InvestEasy.Components.Account;
using InvestEasy.Data;
using InvestEasy.Services;
using InvestEasy.Services.Email;

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
        // New accounts must confirm their email before they can log in.
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;

        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Account emails. Without SMTP settings the links are written to the log in development,
// and the app refuses to start in production, since nobody could confirm their account.
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
var emailOptions = builder.Configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>() ?? new EmailOptions();

if (emailOptions.IsConfigured)
{
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, SmtpEmailSender>();
}
else if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, DevelopmentEmailSender>();
}
else
{
    throw new InvalidOperationException("Email is not configured. Set Email:Host and Email:FromAddress (and Email:Username/Email:Password), see README.md.");
}

// Limits how often one IP address can submit the account forms (log in, register, forgot password and so on).
// Stops password guessing across many accounts and keeps the app from being used to send lots of emails.
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        if (!HttpMethods.IsPost(context.Request.Method) ||
            !context.Request.Path.StartsWithSegments("/Account", StringComparison.OrdinalIgnoreCase))
        {
            return RateLimitPartition.GetNoLimiter("unlimited");
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // The login page asks for passkey options in the background every time it loads,
        // so those calls get their own, larger limit and don't use up the form submissions.
        if (context.Request.Path.Value?.Contains("Passkey", StringComparison.OrdinalIgnoreCase) == true)
        {
            return RateLimitPartition.GetFixedWindowLimiter("passkey:" + ip,
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1) });
        }

        return RateLimitPartition.GetFixedWindowLimiter("forms:" + ip,
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1) });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("Too many attempts. Please wait a minute and try again.", cancellationToken);
    };
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

// Links in account emails are built from the request's host name. AllowedHosts makes sure that
// is the real domain, otherwise someone could get the app to email links to their own site.
if (!app.Environment.IsDevelopment() && app.Configuration["AllowedHosts"] is null or "*")
{
    app.Logger.LogWarning("AllowedHosts is \"*\". Set it to the app's domain in production, see README.md.");
}

// Configure the HTTP request pipeline.
// Behind a reverse proxy, set ASPNETCORE_FORWARDEDHEADERS_ENABLED=true so the app sees the original
// scheme and client IP. ASP.NET Core then adds the forwarded headers middleware itself.

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

// Security headers for every response. The Content Security Policy only allows scripts from the app itself,
// Bootstrap Icons from jsDelivr and news images from any https site.
const string contentSecurityPolicy =
    "default-src 'self'; " +
    "script-src 'self'; " +
    "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
    "font-src 'self' https://cdn.jsdelivr.net; " +
    "img-src 'self' data: https:; " +
    "connect-src 'self'; " +
    "frame-ancestors 'none'; " +
    "base-uri 'self'; " +
    "form-action 'self'";

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["Content-Security-Policy"] = contentSecurityPolicy;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.UseRateLimiter();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHealthChecks("/health");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
