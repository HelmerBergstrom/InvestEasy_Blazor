# InvestEasy

This is a web application buillt with **ASP.NET Core and Blazor** that focuses on helping users with exploring 
financial markets, tracking stocks, learning investing concepts and analyzing saving growth over time.
The application combines market data, educational content and practical financial tools in one app.

--------

## Tech

The app is build using the following tevhnologies and tools:

- ASP.NET Core / Blazor Server
- C#
- Entity Framework Core - Database interaction
- SQLite - Database
- Finnhub API - financial market data API
- HTML / CSS 

--------

## Funcionality

The app includes these features:

- **Authentication**    
    - User registration and login.
    - Passwords stored with hashing.

- **Saving Calculator**
    - Calculates future value based on monthly savings, starting amount, time horizon and expected annual return.
    - Users may save saving scenarios in the database.
    - Saved scenarios could be edited or removed.
    - Scenarios is linked with users via UserId.

- **Market**
    - Displays financial news using external API.
    - Shows selected market indexes, assets and stocks with relevant numbers for each.
    - Section for a watchlist. Users can search for stocks and add stocks to a personal watchlist.
    - Stocks in the watchlist shows change in percent, change in price and current price.
    - Stocks in the watchlist can be removed.

- **Education / Academy**
    - Multiple learning levels. 
    - Tracks users progress through the levels. 

- **Risk Profile**
    - Questionaire that calculates a recommended asset allocation between stocks, funds and low risk / savings account.
    - Results can be saved and is linked to a single user. 

--------

## Database

The application uses a local **SQLite** database with **Entity Framework Core**.
The information that´s stored in the database:
- User Accounts (via ASP.NET Identity)
- Saved savnings scenarios
- Watchlist items.
- User learning progress.
- Saved risk profiles.

--------

## Running the project

To run this project locally, follow the following steps:

1. Clone the repo
```bash
git clone https://github.com/HelmerBergstrom/InvestEasy_Blazor/tree/main
```

2. Navigate to the folder
```bash
cd InvestEasy
```

3. Restore
```bash
dotnet restore
```

4. Do database migrations
```bash
dotnet ef database update
```

5. Run te app! :D
```bash
dotnet run
```
or
```bash
dotnet watch run
```


