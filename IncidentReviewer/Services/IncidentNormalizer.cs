using System.Globalization;
using IncidentReviewer.Models;

namespace IncidentReviewer.Services
{
    /// <summary>
    /// Raw fields exactly as they appear in source data — no pre-cleaning.
    /// <c>AssignedCrew</c> defaults to <c>null</c> so earlier seeders without crew data still compile.
    /// </summary>
    public record RawIncidentRow(
        string Title,
        string Category,
        string Status,
        string Timestamp,
        int? ResponseTimeMinutes,
        string? AssignedCrew = null);

    public static class IncidentNormalizer
    {
        private static readonly string[] TimestampFormats =
        [
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",    // N2: with seconds
            "yyyy-MM-ddTHH:mm",       // N1: ISO 8601 T-separator
            "yyyy-MM-ddTHH:mm:ss",    // N1: ISO 8601 T-separator with seconds
            "MM/dd/yyyy HH:mm",
            "MM/dd/yyyy HH:mm:ss",    // N2: US format with seconds
            "yyyy-MM-dd",
            "MM/dd/yyyy",
        ];

        public static Incident Normalize(RawIncidentRow raw) => new()
        {
            Title               = NormalizeTitle(raw.Title),
            Category            = NormalizeWord(raw.Category),
            Status              = ParseStatus(raw.Status),
            Timestamp           = ParseTimestamp(raw.Timestamp),
            ResponseTimeMinutes = raw.ResponseTimeMinutes,
            AssignedCrew        = NormalizeCrew(raw.AssignedCrew),
        };

        // Capitalises the first character; leaves the rest unchanged.
        // "medical assist" → "Medical assist", "Vehicle Accident" stays "Vehicle Accident".
        public static string NormalizeTitle(string title)
        {
            title = title.Trim();
            return title.Length == 0 ? title : char.ToUpperInvariant(title[0]) + title[1..];
        }

        // Title-cases a single word: "medical" → "Medical", "Fire" → "Fire".
        public static string NormalizeWord(string word)
        {
            word = word.Trim();
            return word.Length == 0 ? word : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant());
        }

        // Normalises crew identifiers to "Name-Number" format.
        // Handles lowercase ("alpha-3" → "Alpha-3"), space separator ("Alpha 3" → "Alpha-3"),
        // and slash separator ("Alpha/3" → "Alpha-3") — N5.
        public static string? NormalizeCrew(string? crew)
        {
            if (string.IsNullOrWhiteSpace(crew)) return null;

            var parts = crew.Trim().Split(['-', ' ', '/'], StringSplitOptions.RemoveEmptyEntries);
            return string.Join("-", parts.Select(p =>
                char.IsLetter(p[0]) ? NormalizeWord(p) : p));
        }

        // Case-insensitive parse of "Active"/"active", "Resolved"/"resolved"/"RESOLVED".
        private static IncidentStatus ParseStatus(string raw)
        {
            if (Enum.TryParse<IncidentStatus>(raw.Trim(), ignoreCase: true, out var status))
                return status;

            throw new FormatException($"Unrecognised incident status: '{raw}'");
        }

        private static DateTime ParseTimestamp(string raw)
        {
            raw = raw.Trim();
            if (DateTime.TryParseExact(raw, TimestampFormats,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            // Fall back to general parse for any formats not explicitly listed.
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;

            throw new FormatException($"Unrecognised timestamp format: '{raw}'");
        }
    }
}
