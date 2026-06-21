# Session Log — 200-Row Seed Data

**Date:** 2026-06-22

## What was done

Created `BaseDataSeederV3` (200 rows total) and updated `IncidentNormalizer` to handle 5 new malformed-data patterns.

### Files changed

| File | Change |
|------|--------|
| `Data/BaseDataSeederV3.cs` | New — 200-row seeder (rows 1–8 from image.png, 9–200 generated) |
| `Services/IncidentNormalizer.cs` | Added 4 timestamp formats + slash separator in `NormalizeCrew` |
| `Program.cs` | Changed `BaseDataSeederV2.Seed` → `BaseDataSeederV3.Seed` |

### Seed strategy

`BaseDataSeederV3.Seed` checks `db.Incidents.Count() >= 200`. If fewer than 200 exist (including the 8 from V2), it clears and reseeds all 200. If ≥ 200 already present, it skips.

## Row structure

- **Rows 1–8**: Real incidents from image.png, verbatim (E1–E6 already present)
- **Rows 9–60**: 52 clean generated rows (March 7 – April 1, 2024)
- **Rows 61–90**: 30 rows exercising existing patterns E1–E6
- **Rows 91–110**: 20 rows with ISO 8601 T-separator timestamps (N1)
- **Rows 111–130**: 20 rows with timestamps including seconds (N2)
- **Rows 131–150**: 20 rows with leading/trailing whitespace on text fields (N3)
- **Rows 151–170**: 20 rows with ALL-CAPS category (N4)
- **Rows 171–190**: 20 rows with slash-separated crew (N5)
- **Rows 191–200**: 10 rows combining multiple new + old patterns

## Malformed data patterns

### Existing (E-series)
| Code | Pattern | Example |
|------|---------|---------|
| E1 | Lowercase category/status/crew | `"medical"`, `"resolved"`, `"alpha-3"` |
| E2 | ALL-CAPS status | `"RESOLVED"`, `"ACTIVE"` |
| E3 | Date-only timestamp | `"2024-03-05"` |
| E4 | Outlier response time | `999` |
| E5 | Space crew separator | `"Alpha 3"` |
| E6 | Null crew / null response time | `null` |

### New (N-series)
| Code | Pattern | Example | Normalizer fix |
|------|---------|---------|----------------|
| N1 | ISO 8601 T-separator | `"2024-04-15T10:30"`, `"2024-04-15T10:30:00"` | Added `"yyyy-MM-ddTHH:mm"` and `"yyyy-MM-ddTHH:mm:ss"` to `TimestampFormats` |
| N2 | Timestamp with seconds | `"2024-05-10 09:15:30"` | Added `"yyyy-MM-dd HH:mm:ss"` and `"MM/dd/yyyy HH:mm:ss"` to `TimestampFormats` |
| N3 | Whitespace-padded fields | `"  Medical  "`, `"  Resolved  "`, `"  Alpha-2  "` | Already handled by `Trim()` in each normalizer method |
| N4 | ALL-CAPS category | `"FIRE"`, `"MEDICAL"`, `"HAZMAT"` | Already handled by `NormalizeWord` (lowercases before title-casing) |
| N5 | Slash-separated crew | `"Alpha/3"`, `"bravo/2"` | Added `'/'` to split chars in `NormalizeCrew` |
