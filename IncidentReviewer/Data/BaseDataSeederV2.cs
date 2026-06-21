using IncidentReviewer.Services;

namespace IncidentReviewer.Data
{
    /// <summary>
    /// Seeds the 8 real incidents captured in image.png (rows 1–8 of the source dataset),
    /// including <c>AssignedCrew</c> values. Raw data quality issues are preserved verbatim;
    /// normalisation (casing, separator, timestamp format) is handled by <see cref="IncidentNormalizer"/>.
    /// </summary>
    public static class BaseDataSeederV2
    {
        // Raw rows copied verbatim from image.png — all data quality issues intentionally preserved.
        private static readonly RawIncidentRow[] SourceRows =
        [
            //                title                           category   status      timestamp           response  crew
            new("Structure fire - residential", "Fire",    "Resolved", "2024-03-01 14:22", 8,    "Alpha-3"),
            new("Vehicle accident - highway",   "Rescue",  "Resolved", "2024-03-02 09:11", 12,   "Bravo-1"),
            new("Medical assist",               "Medical", "Resolved", "2024-03-02 11:45", 6,    "Alpha-3"),
            new("Structure fire - commercial",  "Fire",    "Active",   "2024-03-03 08:00", null,  null),           // no crew assigned
            new("Hazmat spill",                 "Hazmat",  "Resolved", "2024-03-03 15:30", 24,   "Charlie-2"),
            new("medical assist",               "medical", "resolved", "03/04/2024 08:22", 6,    "alpha-3"),       // lowercase category, status, and crew
            new("Vehicle Accident",             "Rescue",  "RESOLVED", "2024-03-05",       999,  "Bravo-1"),      // all-caps status, date-only timestamp, outlier response time
            new("Structure fire - residential", "Fire",    "Resolved", "2024-03-06 10:15", 7,    "Alpha 3"),      // crew uses space instead of hyphen
        ];

        public static void Seed(IncidentDbContext db)
        {
            if (db.Incidents.Any()) return;

            db.Incidents.AddRange(SourceRows.Select(IncidentNormalizer.Normalize));
            db.SaveChanges();
        }
    }
}
