# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

InvestEasy is a Blazor Server web app (.NET 10, ASP.NET Core Identity, EF Core + SQLite) for exploring markets, tracking a stock watchlist, learning investing basics, calculating savings growth and building a risk profile. Market data comes from the Finnhub API. It was started as a school project (DT191G) and is being developed further. GitHub: `HelmerBergstrom/InvestEasy_Blazor`.

## Commands

```bash
dotnet restore
dotnet build
dotnet watch run                               # dev server with hot reload
dotnet ef database update                      # apply migrations to Data/app.db
dotnet ef migrations add <Name> -o Data/Migrations
```

There is no test project and no linter configured.

## Configuration

`appsettings.json` and `appsettings.Development.json` are **gitignored**, so a fresh clone must create them. `Program.cs` requires:
- `ConnectionStrings:DefaultConnection`: locally `DataSource=Data/app.db;Cache=Shared`
- `Finnhub:BaseUrl` (`https://finnhub.io/api/v1/`) and `Finnhub:ApiKey`

`*.db` files are also gitignored. The SQLite database is created locally by `dotnet ef database update`.

## Architecture

- **Render mode:** `App.razor` does not set a global render mode. Each interactive page opts in with `@rendermode InteractiveServer`. Pages that need a logged-in user add `@attribute [Authorize]`. `Routes.razor` uses `AuthorizeRouteView` and redirects unauthorized users to login.
- **Services** (`Components/Services/`, namespace `InvestEasy.Services`) are all registered as scoped in `Program.cs`, and pages call them rather than the DbContext directly (exception: `RiskProfile.cs` also injects `ApplicationDbContext`). A new service has to be registered in `Program.cs`.
  - `MarketService` calls Finnhub through the named `HttpClient` `"FinnhubClient"`. It appends `token={apiKey}` to every query string and caches quotes in `IMemoryCache` for 60 seconds.
  - `CurrentUserService.GetUserIdAsync()` is how pages get the current user's Identity id (`ClaimTypes.NameIdentifier`). Every user-owned entity is filtered by `UserId`.
  - `ICalculatorService`/`CalculatorService` is pure compound-interest math with monthly compounding and no dependencies.
- **Data:** `ApplicationDbContext` extends `IdentityDbContext<ApplicationUser>`. The entity classes are in `Components/Models/Entities/` but use the namespace `InvestEasy.Models` and the suffix `*Model`, for example `WatchListItemModel`. `ApplicationUser` holds navigation collections to them. Finnhub DTOs are in `Components/Models/Dtos/`.
- **Pages** are grouped by feature under `Components/Pages/`: `Market/` (Indexes, News, Stocks and WatchList, with a shared `MarketMenu`), `SavingCalculator/`, `RiskProfile/` and `Education/`. Larger pages move their logic into a code-behind partial class (`*.razor.cs` or `RiskProfile.cs`). Styling uses scoped `*.razor.css` files plus `wwwroot/app.css`, together with Bootstrap 5 and Blazor.Bootstrap.
- **Education levels:** each level has a routed wrapper page (`LevelN.razor`) and a static content component (`LevelNContent.razor`). Unlocking is stored in `UserProgressModel` as `CurrentLevel` plus the flags `Level1Completed` to `Level3Completed`, and the flags are hard-coded in `UserProgressService.CompleteLevelAsync`. Adding a level means changing the entity, adding a migration and updating that switch.
- `Components/Account/` is the scaffolded ASP.NET Identity UI. Leave it mostly untouched.

## Conventions

- Code comments and the README are written in English. Commit messages are written in Swedish.
