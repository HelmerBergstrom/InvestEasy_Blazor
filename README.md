# InvestEasy

A web application built with **ASP.NET Core and Blazor** that helps users explore financial markets, track stocks,
learn investing concepts and analyze how savings grow over time. It combines market data, educational content and
practical financial tools in one app.

> InvestEasy is intended for educational purposes only. Nothing in the app is financial advice.

--------

## Tech

- ASP.NET Core / Blazor Server (.NET 10)
- C#
- ASP.NET Core Identity – accounts and login
- Entity Framework Core – database access
- SQLite – database
- Finnhub API – financial market data
- HTML / CSS with Bootstrap 5 and Bootstrap Icons

--------

## Features

- **Accounts**
    - Registration and login. Passwords are stored hashed.
    - Accounts are locked for 15 minutes after 5 failed login attempts.
    - Passkeys and two-factor authentication are supported.

- **Saving Calculator**
    - Calculates future value from monthly savings, starting amount, time horizon and expected annual return.
    - Shows growth year by year in a chart, with a table view.
    - Scenarios can be saved, edited and deleted (up to 50 per user).

- **Market**
    - General market news.
    - Major indexes, commodities, bitcoin and popular stocks with daily change.
    - Personal watchlist: search for stocks and follow up to 20 of them.

- **Academy**
    - Three learning levels. Each level is unlocked by completing the previous one.

- **Risk Profile**
    - A questionnaire that suggests an allocation between stocks, funds and low risk savings.
    - The result can be saved to the user's account.

--------

## Database

The app uses a **SQLite** database with **Entity Framework Core**. The database is created and migrated
automatically when the app starts. It stores:

- User accounts (ASP.NET Core Identity)
- Saved saving scenarios
- Watchlist items
- Learning progress
- Saved risk profiles

--------

## Running the project locally

1. Clone the repo and go to the folder
```bash
git clone https://github.com/HelmerBergstrom/InvestEasy_Blazor.git
cd InvestEasy_Blazor
```

2. Get a free API key from [Finnhub](https://finnhub.io/register) and store it with user-secrets,
   so it never ends up in the repo
```bash
dotnet user-secrets set "Finnhub:ApiKey" "<your-api-key>"
```

3. Run the app. The database is created in `Data/app.db` on the first start.
```bash
dotnet run
```
or with hot reload
```bash
dotnet watch run
```

--------

## Configuration

| Setting | Default | Description |
| --- | --- | --- |
| `Finnhub:ApiKey` | *(empty)* | **Required for market data.** Use user-secrets locally and an environment variable in production. |
| `Finnhub:BaseUrl` | `https://finnhub.io/api/v1/` | Finnhub API address. |
| `ConnectionStrings:DefaultConnection` | `DataSource=Data/app.db;Cache=Shared` | SQLite database file. |

In environment variables, `:` is written as `__`, for example `Finnhub__ApiKey`.

--------

## Deployment

The app runs anywhere that can run .NET 10 or a Docker container. Things to know:

- **Persistent storage:** the SQLite database and the data protection keys (used to encrypt login cookies) are
  stored in the `Data/` folder. That folder has to survive restarts and redeploys, otherwise all users and their
  data are lost and everyone is logged out.
- **One instance:** Blazor Server keeps a live connection per user and SQLite is a single file, so run one instance.
- **HTTPS:** the app trusts `X-Forwarded-Proto` from a reverse proxy, so the host can terminate HTTPS.
- **Health check:** `GET /health` returns `Healthy` when the app is running.
- **Rate limit:** Finnhub's free plan allows 60 calls per minute. Responses are cached (quotes 1 min, news 5 min,
  searches 30 min) and shared between users.

### With Docker

```bash
docker build -t investeasy .
docker run -d -p 8080:8080 \
  -e Finnhub__ApiKey=<your-api-key> \
  -v investeasy-data:/app/Data \
  investeasy
```

### Without Docker

```bash
dotnet publish -c Release -o ./publish
cd publish
Finnhub__ApiKey=<your-api-key> ASPNETCORE_URLS=http://0.0.0.0:8080 dotnet InvestEasy.dll
```

--------

Market data provided by [Finnhub](https://finnhub.io).
