using IncidentReviewer.Services;

namespace IncidentReviewer.Data
{
    /// <summary>
    /// Seeds the 8 real incidents captured in image.png without <c>AssignedCrew</c>.
    /// Superseded by <see cref="BaseDataSeederV2"/> which adds crew data.
    /// </summary>
    public static class BaseDataSeederV1
    {
        // Raw rows copied verbatim from image.png — capitalisation, timestamp format,
        // and response times are intentionally left as they appear in the source.
        private static readonly RawIncidentRow[] SourceRows =
        [
            new("Structure fire - residential", "Fire",    "Resolved", "2024-03-01 14:22", 8),
            new("Vehicle accident - highway",   "Rescue",  "Resolved", "2024-03-02 09:11", 12),
            new("Medical assist",               "Medical", "Resolved", "2024-03-02 11:45", 6),
            new("Structure fire - commercial",  "Fire",    "Active",   "2024-03-03 08:00", null),
            new("Hazmat spill",                 "Hazmat",  "Resolved", "2024-03-03 15:30", 24),
            new("medical assist",               "medical", "resolved", "03/04/2024 08:22", 6),    // lowercase category + status, MM/dd/yyyy timestamp
            new("Vehicle Accident",             "Rescue",  "RESOLVED", "2024-03-05",       999),   // all-caps status, date-only timestamp, outlier response time
            new("Structure fire - residential", "Fire",    "Resolved", "2024-03-06 10:15", 7),
        ];

        public static void Seed(IncidentDbContext db)
        {
            if (db.Incidents.Any()) return;

            db.Incidents.AddRange(SourceRows.Select(IncidentNormalizer.Normalize));
            db.SaveChanges();
        }
    }
}
