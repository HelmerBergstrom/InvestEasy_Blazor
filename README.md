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
    - Registration with email confirmation, login and password reset by email. Passwords are stored hashed.
    - Accounts are locked for 15 minutes after 5 failed login attempts, and the account forms are rate limited per IP.
    - Passkeys and two-factor authentication are supported.
    - Users can download all their data and delete their account, which deletes everything they have saved.

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
   Without email settings, confirmation and password reset links are written to the console instead of being sent.
```bash
dotnet run
```
or with hot reload
```bash
dotnet watch run
```

4. Run the tests
```bash
dotnet test
```

--------

## Configuration

| Setting | Default | Description |
| --- | --- | --- |
| `Finnhub:ApiKey` | *(empty)* | **Required for market data.** Use user-secrets locally and an environment variable in production. |
| `Finnhub:BaseUrl` | `https://finnhub.io/api/v1/` | Finnhub API address. |
| `ConnectionStrings:DefaultConnection` | `DataSource=Data/app.db;Cache=Shared` | SQLite database file. |
| `Email:Host` | *(empty)* | **Required in production.** SMTP server, ex. `smtp-relay.brevo.com`. |
| `Email:Port` | `587` | `587` (STARTTLS) or `465` (TLS). |
| `Email:Username` / `Email:Password` | *(empty)* | SMTP login. The password is a secret, set it as an environment variable. |
| `Email:FromAddress` | *(empty)* | **Required in production.** Sender address, ex. `noreply@yourdomain.com`. |
| `Email:FromName` | `InvestEasy` | Sender name. |
| `Site:ContactEmail` | *(empty)* | Shown on the privacy page as the contact for questions about personal data. |
| `AllowedHosts` | `*` | **Set to your domain in production**, ex. `investeasy.se`. See below. |

In environment variables, `:` is written as `__`, for example `Finnhub__ApiKey`.

--------

## Deployment

The app runs anywhere that can run .NET 10 or a Docker container.

### Checklist

1. A Finnhub API key → `Finnhub__ApiKey`.
2. An SMTP provider for account emails (Brevo, Mailgun, Resend, Postmark and others have free plans) →
   `Email__Host`, `Email__Port`, `Email__Username`, `Email__Password`, `Email__FromAddress`.
   Verify the sender domain with the provider (SPF/DKIM), otherwise the emails end up as spam.
   **The app refuses to start in production without email settings**, since new users couldn't confirm their accounts.
3. `AllowedHosts` → your domain. Links in account emails are built from the request's host name, so this stops
   anyone from getting the app to email password reset links that point to their own site.
4. `Site__ContactEmail` → your contact address for the privacy page.
5. A persistent volume for `Data/` (see below), and regular backups of `Data/app.db`.
6. Behind a reverse proxy that handles HTTPS (most hosting platforms), set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`.
   The Dockerfile already does. Without it, HTTPS redirects can loop and rate limiting sees the proxy's IP.

### Things to know

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
  -e Email__Host=<smtp-host> -e Email__Username=<smtp-user> -e Email__Password=<smtp-password> \
  -e Email__FromAddress=noreply@<your-domain> \
  -e AllowedHosts=<your-domain> \
  -v investeasy-data:/app/Data \
  investeasy
```

### Without Docker

```bash
dotnet publish -c Release -o ./publish
cd publish
# Set the same environment variables as in the Docker example, then:
ASPNETCORE_URLS=http://0.0.0.0:8080 dotnet InvestEasy.dll
```

### Backups

SQLite is a single file. A safe copy can be taken while the app is running with:
```bash
sqlite3 Data/app.db ".backup 'app-backup.db'"
```

--------

## Security

- Passwords are hashed by ASP.NET Core Identity. Accounts lock after 5 failed logins.
- Account forms are limited to 20 submissions per minute per IP address.
- Email must be confirmed before logging in. Password reset and "forgot password" never reveal whether an email has an account.
- Every user-owned row is filtered by the logged-in user's id, and has a foreign key with cascade delete.
- Content Security Policy, `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy` and HSTS headers.
- Secrets (API key, SMTP password) are never committed. Use user-secrets locally and environment variables in production.

--------

Market data provided by [Finnhub](https://finnhub.io).
