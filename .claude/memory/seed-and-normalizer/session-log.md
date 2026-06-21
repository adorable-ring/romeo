# Session Log — Seed Data & Normalizer

## What was accomplished

Added a normalisation layer, real seed data from `image.png`, and the `AssignedCrew` field.

### Files created

| File | What it contains |
|------|-----------------|
| `Services/IncidentNormalizer.cs` | `RawIncidentRow` record + static `IncidentNormalizer` class — normalises raw source strings into clean `Incident` objects |
| `Data/BaseDataSeederV1.cs` | Renamed from `BaseDataSeeder.cs`; 8 real rows from `image.png` without `AssignedCrew` — kept as legacy reference |
| `Data/BaseDataSeederV2.cs` | 8 real rows from `image.png` with raw `AssignedCrew` values verbatim; currently active seeder called from `Program.cs` |
| `Migrations/0002_AddAssignedCrew.cs` | EF migration adding nullable `AssignedCrew TEXT` column |

### Files modified

| File | Change |
|------|--------|
| `Models/Incident.cs` | Added `AssignedCrew string?`; added `OutlierThresholdMinutes = 300` const; added `[NotMapped] IsResponseTimeOutlier` computed property |
| `Models/Incident.cs` | `CreateIncidentDto` updated to include `string? AssignedCrew` |
| `Controllers/IncidentsController.cs` | `Map()` passes `AssignedCrew` through (trimmed, null if blank) |
| `Program.cs` | Startup now calls `BaseDataSeederV2.Seed(db)` after `Database.Migrate()` |

### Normalizer capabilities (`IncidentNormalizer`)

| Input type | Example raw → normalised |
|------------|--------------------------|
| Status casing | `"resolved"`, `"RESOLVED"` → `IncidentStatus.Resolved` |
| Category casing | `"medical"` → `"Medical"` |
| Title casing | `"medical assist"` → `"Medical assist"` (first char only) |
| Timestamp: `yyyy-MM-dd HH:mm` | `"2024-03-01 14:22"` → DateTime |
| Timestamp: `MM/dd/yyyy HH:mm` | `"03/04/2024 08:22"` → DateTime |
| Timestamp: date-only | `"2024-03-05"` → DateTime (00:00) |
| Crew casing | `"alpha-3"` → `"Alpha-3"` |
| Crew separator | `"Alpha 3"` → `"Alpha-3"` |
| Missing crew | `null` / blank → `null` |

### Outlier handling decision

`IsResponseTimeOutlier` is `[NotMapped]` — computed at runtime as `ResponseTimeMinutes > Incident.OutlierThresholdMinutes` (300 min). No DB column. Threshold is a `const` on the `Incident` class so the model owns the definition; the normalizer and future aggregation code reference it from there.

### Seeder versioning pattern

V1 = real rows without crew (kept for reference, not called).
V2 = real rows with crew (currently active). Future: `GeneratedDataSeeder` for the 200-row synthetic set.

### What remains open

- `GeneratedDataSeeder` (synthetic ~192 rows to bring total to 200, with same data quality issues).
- Category filter in GET is still exact-match — may need case-insensitive comparison once mixed-case data is in place.
- No test project yet.
