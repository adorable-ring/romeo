---
name: api-normalisation-fixes-session
description: Fixes for Swagger enum display (0/1 vs Active/Resolved) and case-sensitive category filter returning empty results; full normalisation wired into controller
metadata:
  type: project
---

## What was fixed

### 1. Swagger showing status as `0` / `1` instead of `"Active"` / `"Resolved"`
- **Root cause:** Default ASP.NET Core JSON serialisation renders enums as integers.
- **Fix in `Program.cs`:**
  ```csharp
  builder.Services.AddControllers()
      .AddJsonOptions(options =>
          options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
  builder.Services.AddSwaggerGen(options =>
      options.UseInlineDefinitionsForEnums());
  ```
- `JsonStringEnumConverter` makes all enum fields serialise/deserialise as strings everywhere (responses, request bodies, Swagger).
- `UseInlineDefinitionsForEnums()` tells Swashbuckle to render the string values in the UI dropdown.

### 2. Category filter returning empty results (`?category=medical` → `[]`)
- **Root cause:** SQLite uses binary (case-sensitive) collation for `=` by default. Data is stored as `"Medical"` (normalised on ingest) but the filter compared against the raw query string value `"medical"`.
- **Fix in `IncidentsController.Get()`:** normalise the incoming filter value before querying:
  ```csharp
  query = query.Where(i => i.Category == IncidentNormalizer.NormalizeWord(category));
  ```

### 3. Full normalisation in controller `Map()`
Previously `Map()` only normalised `Category`; `Title` was just trimmed and `AssignedCrew` was trimmed but not crew-formatted.

- `IncidentNormalizer.NormalizeTitle`, `NormalizeWord`, `NormalizeCrew` promoted from `private` to `public` — single source of truth.
- `Map()` now delegates to them:
  ```csharp
  Title        = IncidentNormalizer.NormalizeTitle(dto.Title),
  Category     = IncidentNormalizer.NormalizeWord(dto.Category),
  AssignedCrew = IncidentNormalizer.NormalizeCrew(dto.AssignedCrew),
  ```
- Local `Normalise()` helper in the controller removed.

## Files changed
- `IncidentReviewer/Program.cs` — enum serialisation + Swagger config
- `IncidentReviewer/Controllers/IncidentsController.cs` — normalise category filter + Map() delegates to IncidentNormalizer
- `IncidentReviewer/Services/IncidentNormalizer.cs` — NormalizeTitle, NormalizeWord, NormalizeCrew made public

## Outstanding
- Status filter via query string works (ASP.NET Core's enum model binding is case-insensitive by default).
- No tests yet; both bugs above would have been caught by integration tests.
