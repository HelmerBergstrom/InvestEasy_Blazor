# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

InvestEasy is a Blazor Server web app (.NET 10, ASP.NET Core Identity, EF Core + SQLite) for exploring markets, tracking a stock watchlist, learning investing basics, calculating savings growth and building a risk profile. Market data comes from the Finnhub API. It was started as a school project (DT191G) and is being developed further. GitHub: `HelmerBergstrom/InvestEasy_Blazor`.

## Commands

```bash
dotnet restore
dotnet build
dotnet watch run                               # dev server with hot reload
dotnet ef database update                      # apply migrations manually (the app also migrates on startup)
dotnet ef migrations add <Name> -o Data/Migrations --project InvestEasy.csproj
dotnet test                                    # xUnit tests in tests/InvestEasy.Tests
```

Tests use an in-memory SQLite database with the real migrations (`TestDatabase`), so foreign keys and unique indexes are enforced. The web project excludes `tests/**` from compilation. CI (`.github/workflows/ci.yml`) builds and tests on push. There is no linter.

## Configuration

`appsettings.json` is committed and holds no secrets. `Finnhub:ApiKey` and `Email:Password` are set with `dotnet user-secrets` locally and as environment variables (`Finnhub__ApiKey`, `Email__Password`) in production. Without a Finnhub key the app runs but market data is unavailable.

Email (`Email:*`, `Services/Email/`): `SmtpEmailSender` (MailKit) is used when `Email:Host` and `Email:FromAddress` are set. Without them, `DevelopmentEmailSender` logs the links in development, and the app throws at startup in other environments, because `RequireConfirmedAccount` is true. `AllowedHosts` must be the real domain in production, since email links are built from the request host.
- `ConnectionStrings:DefaultConnection`: `DataSource=Data/app.db;Cache=Shared`
- `Finnhub:BaseUrl` (`https://finnhub.io/api/v1/`) and `Finnhub:ApiKey`

`Program.cs` creates the database folder, runs `Database.Migrate()` on startup, stores data protection keys in `Data/keys/`, sets security headers (including a Content Security Policy: scripts only from 'self'), and rate limits POSTs to `/Account/*` per IP (passkey option calls have their own limit). `*.db` files and `Data/keys/` are gitignored. Deployment (Dockerfile, persistent `Data/` folder, `/health`) is described in README.md.

## Architecture

- **Render mode:** `App.razor` does not set a global render mode. Each interactive page opts in with `@rendermode InteractiveServer`. Pages that need a logged-in user add `@attribute [Authorize]`. `Routes.razor` uses `AuthorizeRouteView` and redirects unauthorized users to login.
- **Services** (`Components/Services/`, namespace `InvestEasy.Services`) are all registered as scoped in `Program.cs`, and pages call them rather than the DbContext directly. A new service has to be registered in `Program.cs`.
  - Services that use the database inject `IDbContextFactory<ApplicationDbContext>` and create a context per method (`await using var db = ...`), because a scoped service lives as long as the Blazor circuit. Identity still uses the scoped `ApplicationDbContext`.
  - `MarketService` calls Finnhub through the named `HttpClient` `"FinnhubClient"`, sends the key in the `X-Finnhub-Token` header and caches every response in `IMemoryCache` (quotes 60 s, news 5 min, searches 30 min). Quote failures return null; news and search failures throw so the page can show an error.
  - `WatchListService` and `SavingScenarioService` cap items per user (20 and 50).
  - `CurrentUserService.GetUserIdAsync()` is how pages get the current user's Identity id (`ClaimTypes.NameIdentifier`). Every user-owned entity is filtered by `UserId`.
  - `ICalculatorService`/`CalculatorService` is pure compound-interest math with monthly compounding and no dependencies.
- **Data:** `ApplicationDbContext` extends `IdentityDbContext<ApplicationUser>`. `OnModelCreating` makes `UserId` the foreign key to the user with cascade delete, and adds unique indexes (watchlist `UserId`+`Symbol`, one progress and one risk profile per user). Services catch `DbUpdateException` from those indexes. The entity classes are in `Components/Models/Entities/` but use the namespace `InvestEasy.Models` and the suffix `*Model`, for example `WatchListItemModel`. `ApplicationUser` holds navigation collections to them. Finnhub DTOs are in `Components/Models/Dtos/`.
- **Pages** are grouped by feature under `Components/Pages/`: `Market/` (Indexes, News, Stocks and WatchList, with a shared `MarketMenu`), `SavingCalculator/`, `RiskProfile/` and `Education/`. Larger pages move their logic into a code-behind partial class (`*.razor.cs` or `RiskProfile.cs`). Styling uses scoped `*.razor.css` files plus `wwwroot/app.css`, together with Bootstrap 5 (CSS only, from `wwwroot/lib`) and Bootstrap Icons from a CDN.
- **Education levels:** each level has a routed wrapper page (`LevelN.razor`) and a static content component (`LevelNContent.razor`). Unlocking is stored in `UserProgressModel` as `CurrentLevel` plus the flags `Level1Completed` to `Level3Completed`, and the flags are hard-coded in `UserProgressService.CompleteLevelAsync`. Adding a level means changing the entity, adding a migration and updating that switch. `CompleteLevelAsync` refuses locked levels, and each `LevelN.razor` redirects to `/education` if the level is locked.
- `Components/Account/` is the scaffolded ASP.NET Identity UI. Leave it mostly untouched. The email pages (forgot/reset password, confirm, resend) are restyled with `PageHeader` and panels, never reveal whether an email has an account, and treat broken codes in links as invalid links. Login uses `lockoutOnFailure: true`. `DownloadPersonalData` also exports the user's app data.
- `Components/Pages/Privacy.razor` is the privacy policy (contact from `Site:ContactEmail`), linked from the footer and the register page.

## Conventions

- Code comments and the README are written in English. Commit messages are written in Swedish.
