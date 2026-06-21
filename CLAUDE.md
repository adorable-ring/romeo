# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

ASP.NET Core 8 Web API for an emergency services internal incident logging and review tool. The project is in its initial scaffolded state — the incident domain logic has not yet been built. The `WeatherForecastController` is a template artifact and should be replaced.

## Commands

```powershell
# Build
dotnet build IncidentReviewer/IncidentReviewer.csproj

# Run (http, opens Swagger at http://localhost:5192/swagger)
dotnet run --project IncidentReviewer/IncidentReviewer.csproj --launch-profile http

# Run (https)
dotnet run --project IncidentReviewer/IncidentReviewer.csproj --launch-profile https

# Docker build & run
docker build -t incidentreviewer -f IncidentReviewer/Dockerfile .
docker run -p 8080:8080 incidentreviewer
```

No test project exists yet.

## Domain

**Incident** is the core entity. Fields (derived from source data):
- `Title` — description of the incident
- `Category` — type of incident (e.g. Fire, Medical, Traffic)
- `Timestamp` — when the incident was logged; source data has inconsistent formats and must be normalised on ingest
- `Status` — `Active` or `Resolved`
- `ResponseTime` — duration in minutes; source data contains outliers (e.g. 999 mins) that should be flagged or excluded from aggregations

**Known data quality issues in source data** (must be handled on ingest or seed):
- Inconsistent capitalisation across category and status values
- Multiple timestamp formats
- Empty / unfilled fields
- Outlier response times

## Features to build

1. **Log incidents** — create incidents individually (minor use case) or in batch (primary use case). Both paths write to the same store.
2. **Review and filter incidents** — read incidents as rows; expose filtering via the Swagger/OpenAPI surface. Filter dimensions to be decided (candidates: category, status, date range).
3. **Status tracking** — incidents transition between `Active` and `Resolved` over time; the API must support updating status.

## Seed data

The first 8 rows come from `image.png` in the project root. A representative dataset of 200 rows must be generated that reflects the same data quality issues present in the source (mixed capitalisation, varied timestamp formats, empty fields, outlier response times).

## Architecture

- **`Program.cs`** — minimal hosting model; registers controllers, Swagger/OpenAPI (dev only), and HTTPS redirection.
- **`Controllers/`** — standard MVC controller pattern; `IncidentsController` will be the primary controller.
- **`appsettings.json` / `appsettings.Development.json`** — environment-split configuration; `Development` overrides applied automatically when `ASPNETCORE_ENVIRONMENT=Development`.
- **User Secrets** — configured for `dotnet user-secrets` (ID `7ce63d7e-0105-49ba-bc8a-45f32b0befa5`); use for local connection strings and secrets.
- **Docker** — multi-stage Dockerfile targets Linux (`mcr.microsoft.com/dotnet/aspnet:8.0`); exposes ports 8080 (HTTP) and 8081 (HTTPS).
- **Swagger** — available at `/swagger` in Development via Swashbuckle (`Swashbuckle.AspNetCore` 6.6.2).

## Memory

All session memory lives in `.claude/memory/` inside this repository. Never write memory to any path outside the repo (e.g. do not use `~/.claude/projects/...`).

**Structure:** each session gets its own directory named by topic (e.g. `initial-build/`, `seed-and-normalizer/`). Create `.md` files inside it for detail (architecture decisions, session log, open questions, etc.). Register every new directory and its key files in `.claude/memory/MEMORY.md`.

**Index entry length tiers:**
- Max (critical, high-cost-to-get-wrong decisions): under 2000 chars
- Average (what was built, notable choices, open items): under 750 chars
- Not-very-relevant: under 300 chars

The index entry should be a meaningful summary. Full detail belongs in the linked file, not the index.
