# Incident Reviewer

## What I built and the key decisions I made

* A C#/.NET 8 Web API using Entity Framework Core and SQLite, structured as MVC without a view layer. Explorable via Swagger UI at `/swagger`. Built using Claude Code with a decision trail in `.claude/memory/` to support future development.

* **Normalisation on ingest.** An `IncidentNormalizer` service cleans data before it hits the database — casing, timestamp formats, crew identifiers. This keeps callers from needing to pre-clean their data and makes the API resilient to the kinds of inconsistencies seen in the source.

* **Swagger hints.** Format hints (e.g. `YYYY-MM-DD`) are surfaced on query parameters directly in the Swagger UI, reducing friction for anyone exploring the API.

* **Endpoints:** list incidents with optional filters, get by id, create one, create in batch, and update status. Date range was chosen as the primary filter because operators are most likely to want records from a recent or relevant window rather than the full history. Batch creation is the primary ingest path since operational data typically arrives in volume, not one record at a time.

* Worked from the provided 8 rows before generating the full 200 row seed dataset, so the normaliser was validated against real cases first.

## One decision I would make differently with more time

* Add file-based batch import (e.g. CSV to JSON conversion). The batch endpoint currently expects JSON, which adds a step for operators working from spreadsheets. The right format depends on what the client uses, so it was left open rather than assumed.

## One thing I deliberately left out and why

* A frontend. Different teams or organisations may want the data presented differently — a web dashboard, mobile app, or reporting tool. A clean API leaves that decision open and avoids coupling the backend to assumptions made under time pressure.

## What I noticed about the data and how I handled it

* The source data had mixed capitalisation, multiple timestamp formats, empty fields, and outlier response times (e.g. 999 minutes). The normaliser handles these at ingest so the database holds consistently formatted records, supporting readability and any future analysis.
