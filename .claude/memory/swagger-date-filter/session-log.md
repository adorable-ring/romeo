---
name: swagger-date-filter
description: Changes made to GET /Incidents date filter parameters — DateOnly type, format hint, Scalar trial and revert
metadata:
  type: project
---

## What changed

`from` and `to` query parameters on `GET /Incidents` changed from `DateTime?` to `DateOnly?`.

**Why:** `DateTime?` mapped to `string($date-time)` in OpenAPI and offered no format guidance. `DateOnly?` maps to `string($date)`, which Swagger UI shows as a `string($date)` label, and ASP.NET Core model binding auto-rejects invalid values with a 400 before the action runs.

**How to apply:** If date filter parameters are added elsewhere, use `DateOnly?` for date-only inputs and `DateTime?` only when time-of-day matters.

## Range semantics

`from` maps to `TimeOnly.MinValue` (start of day); `to` maps to `TimeOnly.MaxValue` (end of day). This means selecting a single day as `to` captures all incidents within that day, not just those at midnight.

## Format hint

Enabled XML documentation (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`, `NoWarn 1591`) and wired it into Swashbuckle via `IncludeXmlComments`. Added `<param>` doc comments on the `Get` action — "format: YYYY-MM-DD" appears as the parameter description in Swagger UI.

## Scalar trial and revert

Briefly replaced Swagger UI with `Scalar.AspNetCore` (added package, swapped `UseSwaggerUI()` for `MapScalarApiReference()` pointing at `/swagger/{documentName}/swagger.json`). Reverted at user request — Swagger UI restored, Scalar package removed. Scalar URL would have been `/scalar/v1`.

## Files touched

- `Controllers/IncidentsController.cs` — parameter types + XML doc comment
- `Program.cs` — `IncludeXmlComments` in `AddSwaggerGen`
- `IncidentReviewer.csproj` — `GenerateDocumentationFile`, `NoWarn 1591`
