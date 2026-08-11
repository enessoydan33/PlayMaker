# PlayMaker

ASP.NET Core MVC football data hub: Super Lig standings, top scorers, player search with market values, football news, auth, and community comments.

## Features

- **League standings** — Turkish Super Lig table (CollectAPI; SofaScore fallback if CollectAPI fails)
- **Top scorers (Goal Kings)** — Super Lig goal leaders (SofaScore)
- **Player search / profile** — search players and show profile fields including proposed market value (SofaScore)
- **Football news** — trending football news feed (RapidAPI)
- **Auth & profile** — register, login, profile edit (ASP.NET Core Identity + PostgreSQL)
- **Comments** — league / team / player comments (app database)
- **Match polls** — vote UI exists; depends on live match data being available
- **Wonderkids** — local Excel-based player filter/scout UI (EPPlus), not a live API product

**Live Score:** UI is present, but live match data depends on an active third-party API subscription. Without it, the page shows an unavailable state instead of crashing.

## Tech Stack

- ASP.NET Core 8 MVC (C#)
- Entity Framework Core + PostgreSQL (Npgsql)
- ASP.NET Core Identity
- Razor Views + Bootstrap
- MemoryCache
- EPPlus (Wonderkids Excel import)
- External APIs via RapidAPI / CollectAPI

## External APIs

| Feature | Provider |
|---|---|
| Standings (primary) | CollectAPI |
| Standings (fallback) | SofaScore (RapidAPI) |
| Top scorers | SofaScore (RapidAPI) |
| Player search / detail / market value | SofaScore (RapidAPI) |
| Football news | Football News (RapidAPI) |
| Live scores | LiveScore (RapidAPI) — requires active subscription |

## Configuration

1. Copy `PlayMaker/appsettings.Example.json` → `PlayMaker/appsettings.Development.json`
2. Fill in your own connection string and API keys
3. **Do not commit** `appsettings.Development.json` or any file with real secrets

Required sections:

- `ConnectionStrings:DefaultConnection`
- `SofaScoreApi` (`Host`, `Key`)
- `CollectApi` (`Key` with `apikey ` prefix)
- `FootballNewsApi` (`Host`, `Key`)
- `LiveScoreApi` (`Host`, `Key`) — optional until subscribed

## Setup

```bash
git clone <repo-url>
cd PlayMaker
dotnet restore
# Create PlayMaker/appsettings.Development.json from the Example file
dotnet ef database update --project PlayMaker/PlayMaker.csproj
dotnet run --project PlayMaker/PlayMaker.csproj
```

Requirements: .NET SDK 8, PostgreSQL (e.g. Neon).

## Screenshots

_Add screenshots here before publishing the repo (Home, League, Market, News)._

## Limitations

- Live Score needs a paid/active RapidAPI LiveScore subscription
- SofaScore free plans have monthly request quotas — pages use MemoryCache to reduce calls
- Goal Kings / SofaScore standings fallback are mapped for Super Lig (`tournamentId = 52`)
- Wonderkids reads a local Excel file; it is not an AI or live scouting API

## License

Educational / portfolio project. Add a `LICENSE` file if you need an explicit license.
