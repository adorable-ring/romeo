# Architecture Decisions — Initial Build

## Database: SQLite

SQLite was chosen because the tool is an internal emergency services app with a modest, known user base. It requires zero server infrastructure — no separate DB process to install, configure, or secure. The EF Core SQLite provider supports the full migration workflow identically to SQL Server or Postgres, so switching later is straightforward if load grows. The immediate tradeoff accepted is that SQLite has limited concurrent write throughput, but for an internal review tool this is not a concern at this stage.

Connection string is read from configuration and stored via `dotnet user-secrets` locally (ID `7ce63d7e-0105-49ba-bc8a-45f32b0befa5`), keeping credentials out of source control.

## ORM: Entity Framework Core 8

EF Core 8 is the standard .NET ORM and was the natural fit for an ASP.NET Core 8 project. It provides: migration tooling (`dotnet ef migrations add`), LINQ-based queries (no raw SQL), and DbContext DI lifecycle management. The alternative (Dapper) was not chosen because the query patterns here are simple enough that EF Core's overhead is negligible and migration tooling is more convenient.

## Status stored as string in the database

`HasConversion<string>()` is applied to the `Status` property in `IncidentDbContext.OnModelCreating`. This means the SQLite file stores `"Active"` and `"Resolved"` as text rather than `0` and `1`. Decision rationale: the DB is inspectable directly (useful for an ops/debugging context), and avoids integer magic numbers that become ambiguous if the enum order ever changes. C# still enforces type safety via the `IncidentStatus` enum.

## MVC Controllers, not Minimal APIs

The project was scaffolded with the standard MVC controller pattern and this was kept. Controllers were preferred over Minimal APIs because: (a) the team context implies multiple contributors navigating the codebase, and controllers with attribute-based routing are more discoverable; (b) Swashbuckle's Swagger generation has better out-of-the-box support for controllers; (c) the feature set (multiple routes per resource) fits the controller pattern naturally.

## Category and Title normalisation at the controller layer

A `Normalise()` helper (Title Case: first char upper, rest lower) is applied to `Category` at ingest inside the controller's `Map()` method. `Title` is trimmed. This was placed in the controller rather than in the model or a service layer to keep things simple at this stage — there is only one write path per operation type, so there is no risk of normalisation being bypassed. If a service layer is added later, `Map()` should move there.

## Normalizer service for raw / bulk ingest (`IncidentNormalizer`)

Added in seed-and-normalizer session. `Services/IncidentNormalizer.cs` handles raw source data (e.g. CSV, image-extracted rows) where field values are untrusted strings. It accepts a `RawIncidentRow` record and returns a clean `Incident`. Handles: case-insensitive status parse, title-case for category/crew, first-char-only for title, multiple timestamp formats (`yyyy-MM-dd HH:mm`, `MM/dd/yyyy HH:mm`, date-only variants), crew separator normalisation (`"Alpha 3"` → `"Alpha-3"`). The controller's `Map()` is a separate, simpler path for well-typed API input and was intentionally left unchanged.

## `IsResponseTimeOutlier` — computed, not persisted

`Incident.IsResponseTimeOutlier` is a `[NotMapped]` get-only property computed as `ResponseTimeMinutes > OutlierThresholdMinutes`. The threshold (`300` min) is a `const` on the `Incident` class — single source of truth, no layer dependency required. No DB column. Rationale: the flag is fully derivable from `ResponseTimeMinutes`, storing it would create a redundancy that could go out of sync, and response times in the source data are final (not updated after ingest).

## PATCH endpoint for status updates

Status updates use `PATCH /incidents/{id}/status` with a dedicated `UpdateStatusDto` rather than a full `PUT` on the incident. This prevents callers from accidentally overwriting `Title`, `Category`, or `Timestamp` when they only intend to change status. It also makes the intent explicit in the API surface, which is important given the Swagger-first review workflow.

## Batch as primary, individual create as secondary

Requirements explicitly called out batch ingest as the higher-use case (source data comes in bulk). Both `POST /incidents` (single) and `POST /incidents/batch` share the same `Map()` helper, guaranteeing identical normalisation behaviour regardless of which path is used.

## Auto-migrate on startup

`Database.Migrate()` is called during startup inside a scoped service scope in `Program.cs`. This ensures the SQLite schema is always current without requiring a manual migration step. Acceptable for an internal tool at this stage. If CI/CD pipelines are introduced later, migration should be separated from startup to avoid race conditions in multi-instance deployments.
