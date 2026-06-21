# IncidentReviewer — Memory Index

## How this memory system works

Each session that makes meaningful progress creates a new directory under `.claude/memory/` named by topic (e.g. `initial-build/`, `seed-and-normalizer/`). Add `.md` files inside for detail (architecture, decisions, session log, etc.). Link them here.

**All memory files live in `.claude/memory/` inside this repo — never outside it.**

**Index entry length tiers:**
- **Max** (critical, costly-to-get-wrong decisions): under 2000 chars
- **Average** (what was built, notable choices): under 750 chars
- **Not-very-relevant**: under 300 chars

---

## Architecture & Key Decisions

**[Full detail → initial-build/architecture.md](initial-build/architecture.md)**

Stack: ASP.NET Core 8 Web API + EF Core 8 + SQLite + Swashbuckle. SQLite chosen for zero-infrastructure setup; EF Core for migration tooling and LINQ. Status stored as string in DB for readability. MVC Controllers over Minimal APIs for discoverability and Swagger support. Normalisation (Title Case category, trimmed title) done in a shared `Map()` helper so both single and batch ingest behave identically. PATCH used for status updates to prevent accidental field overwrites. Auto-migrate on startup — acceptable now, revisit if multi-instance deployment is introduced. `IsResponseTimeOutlier` is `[NotMapped]` computed from `ResponseTimeMinutes > Incident.OutlierThresholdMinutes` (300) — no DB column. Raw ingest uses `IncidentNormalizer` (separate from controller `Map()`).

---

## Session Logs

### Initial Build
**[Full detail → initial-build/session-log.md](initial-build/session-log.md)**
Scaffolded domain from scratch: `Incident` model, `IncidentDbContext`, `IncidentsController` (GET with filters, GET by id, POST single, POST batch, PATCH status), EF migration. Seed data and tests remain outstanding.

### Seed Data & Normalizer
**[Full detail → seed-and-normalizer/session-log.md](seed-and-normalizer/session-log.md)**
Added `AssignedCrew` field + migration. Built `IncidentNormalizer` (handles mixed casing, multiple timestamp formats, crew separator). Created `BaseDataSeederV1` (8 real rows, no crew) and `BaseDataSeederV2` (8 real rows with raw crew values from `image.png`, verbatim). `IsResponseTimeOutlier` made `[NotMapped]` computed property. Outstanding: `GeneratedDataSeeder` (~192 synthetic rows), no tests yet.

### API Normalisation Fixes
**[Full detail → api-normalisation-fixes/session-log.md](api-normalisation-fixes/session-log.md)**
Fixed Swagger showing `IncidentStatus` as `0`/`1` — added `JsonStringEnumConverter` + `UseInlineDefinitionsForEnums()` in `Program.cs`. Fixed category filter returning empty results — SQLite binary collation is case-sensitive so `"medical"` never matched stored `"Medical"`; fix normalises the filter input via `IncidentNormalizer.NormalizeWord`. Completed full normalisation in `Map()`: `Title` now uses `NormalizeTitle`, `AssignedCrew` uses `NormalizeCrew`; local `Normalise()` helper removed. `NormalizeTitle`/`NormalizeWord`/`NormalizeCrew` promoted to `public` in `IncidentNormalizer` as single source of truth.

### Swagger Date Filter
**[Full detail → swagger-date-filter/session-log.md](swagger-date-filter/session-log.md)**
Changed `from`/`to` query params on `GET /Incidents` from `DateTime?` to `DateOnly?`. `DateOnly?` maps to `string($date)` in OpenAPI — Swagger shows a format label and model binding auto-rejects invalid input with 400. Range semantics: `from` = start of day, `to` = end of day. Added XML doc comments ("format: YYYY-MM-DD") via `IncludeXmlComments`. Briefly trialled `Scalar.AspNetCore` as a UI replacement for a native date picker, then reverted to Swagger UI at user request.

### 200-Row Seed Data
**[Full detail → seed-200-rows/session-log.md](seed-200-rows/session-log.md)**
Created `BaseDataSeederV3` (200 rows: 8 real + 192 generated, Mar–Jun 2024). Updated `IncidentNormalizer` with 4 new timestamp formats (ISO 8601 T-separator, with-seconds variants) and slash crew separator. New malformed patterns N1–N5 introduced alongside existing E1–E6. Seeder clears and reseeds if row count < 200 (handles upgrade from V2's 8-row state). `Program.cs` updated to call V3.
