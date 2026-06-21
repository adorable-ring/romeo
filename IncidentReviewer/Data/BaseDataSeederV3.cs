using IncidentReviewer.Services;

namespace IncidentReviewer.Data
{
    /// <summary>
    /// Seeds all 200 incidents for the initial dataset.
    /// Rows 1–8 are the real incidents from image.png; rows 9–200 are generated to the same schema.
    ///
    /// Data quality patterns present in the raw data (intentionally preserved for normaliser testing):
    ///
    /// Inherited from source (image.png):
    ///   E1 – Lowercase category, status, and/or crew  (e.g. "medical", "resolved", "alpha-3")
    ///   E2 – ALL-CAPS status                          (e.g. "RESOLVED", "ACTIVE")
    ///   E3 – Date-only timestamp, no time component   (e.g. "2024-03-05")
    ///   E4 – Outlier response time ≥ 300 min          (e.g. 999)
    ///   E5 – Crew separator is a space, not a hyphen  (e.g. "Alpha 3")
    ///   E6 – Missing crew and/or response time        (null)
    ///
    /// New patterns introduced in generated rows:
    ///   N1 – ISO 8601 T-separator timestamps          (e.g. "2024-04-15T10:30" or "2024-04-15T10:30:00")
    ///   N2 – Timestamps that include seconds          (e.g. "2024-05-10 09:15:30")
    ///   N3 – Leading/trailing whitespace on text fields (e.g. "  Medical  ", "  Resolved  ")
    ///   N4 – ALL-CAPS category                        (e.g. "FIRE", "MEDICAL", "HAZMAT")
    ///   N5 – Slash-separated crew identifier          (e.g. "Alpha/3", "bravo/1")
    /// </summary>
    public static class BaseDataSeederV3
    {
        private static readonly RawIncidentRow[] SourceRows =
        [
            // ── Rows 1–8: real incidents from image.png ──────────────────────────────────────────────────────
            //                title                             category    status       timestamp                resp   crew
            new("Structure fire - residential",  "Fire",    "Resolved",  "2024-03-01 14:22",  8,    "Alpha-3"),
            new("Vehicle accident - highway",    "Rescue",  "Resolved",  "2024-03-02 09:11",  12,   "Bravo-1"),
            new("Medical assist",                "Medical", "Resolved",  "2024-03-02 11:45",  6,    "Alpha-3"),
            new("Structure fire - commercial",   "Fire",    "Active",    "2024-03-03 08:00",  null,  null),              // E6
            new("Hazmat spill",                  "Hazmat",  "Resolved",  "2024-03-03 15:30",  24,   "Charlie-2"),
            new("medical assist",                "medical", "resolved",  "03/04/2024 08:22",  6,    "alpha-3"),           // E1
            new("Vehicle Accident",              "Rescue",  "RESOLVED",  "2024-03-05",        999,  "Bravo-1"),           // E2 E3 E4
            new("Structure fire - residential",  "Fire",    "Resolved",  "2024-03-06 10:15",  7,    "Alpha 3"),           // E5

            // ── Rows 9–60: generated — clean data ────────────────────────────────────────────────────────────
            new("Cardiac arrest",                "Medical", "Resolved",  "2024-03-07 07:45",  9,    "Alpha-1"),
            new("Gas leak",                      "Hazmat",  "Resolved",  "2024-03-07 12:30",  18,   "Delta-1"),
            new("Vehicle accident - intersection","Rescue",  "Active",    "2024-03-08 08:15",  null,  "Bravo-2"),         // E6
            new("Kitchen fire",                  "Fire",    "Resolved",  "2024-03-08 19:22",  11,   "Alpha-2"),
            new("Breathing difficulty",          "Medical", "Resolved",  "2024-03-09 06:55",  7,    "Alpha-3"),
            new("Brush fire",                    "Fire",    "Active",    "2024-03-09 14:00",  null,  "Charlie-1"),        // E6
            new("Elevator rescue",               "Rescue",  "Resolved",  "2024-03-10 10:30",  33,   "Bravo-3"),
            new("Chemical spill",                "Hazmat",  "Resolved",  "2024-03-10 16:45",  45,   "Charlie-2"),
            new("Fall injury",                   "Medical", "Resolved",  "2024-03-11 08:10",  8,    "Alpha-4"),
            new("Natural gas odor",              "Hazmat",  "Active",    "2024-03-11 21:05",  null,  "Delta-2"),          // E6
            new("Apartment fire",                "Fire",    "Resolved",  "2024-03-12 03:22",  15,   "Alpha-1"),
            new("Chest pain",                    "Medical", "Resolved",  "2024-03-12 11:40",  6,    "Alpha-3"),
            new("Water rescue",                  "Rescue",  "Resolved",  "2024-03-13 14:20",  27,   "Bravo-1"),
            new("Fuel spill",                    "Hazmat",  "Resolved",  "2024-03-13 09:30",  35,   "Delta-1"),
            new("Stroke",                        "Medical", "Active",    "2024-03-14 08:00",  null,  "Alpha-2"),          // E6
            new("Vehicle fire",                  "Fire",    "Resolved",  "2024-03-14 17:15",  13,   "Alpha-5"),
            new("Overdose",                      "Medical", "Resolved",  "2024-03-15 22:45",  10,   "Alpha-3"),
            new("Vehicle extrication",           "Rescue",  "Resolved",  "2024-03-15 07:30",  40,   "Bravo-2"),
            new("Warehouse fire",                "Fire",    "Active",    "2024-03-16 02:10",  null,  null),               // E6
            new("Diabetic emergency",            "Medical", "Resolved",  "2024-03-16 14:55",  7,    "Alpha-1"),
            new("Confined space rescue",         "Rescue",  "Resolved",  "2024-03-17 11:20",  52,   "Bravo-3"),
            new("Mercury exposure",              "Hazmat",  "Resolved",  "2024-03-17 15:45",  38,   "Charlie-2"),
            new("Allergic reaction",             "Medical", "Resolved",  "2024-03-18 09:15",  9,    "Alpha-4"),
            new("Dumpster fire",                 "Fire",    "Resolved",  "2024-03-18 20:30",  6,    "Alpha-2"),
            new("Pedestrian struck",             "Rescue",  "Resolved",  "2024-03-19 16:20",  14,   "Bravo-1"),
            new("Pesticide spill",               "Hazmat",  "Active",    "2024-03-19 08:00",  null,  "Delta-2"),          // E6
            new("Unconscious person",            "Medical", "Resolved",  "2024-03-20 23:10",  11,   "Alpha-3"),
            new("Electrical fire",               "Fire",    "Resolved",  "2024-03-20 14:30",  19,   "Alpha-1"),
            new("Trench rescue",                 "Rescue",  "Resolved",  "2024-03-21 10:45",  65,   "Bravo-2"),
            new("Cardiac arrest",                "Medical", "Resolved",  "2024-03-21 18:25",  8,    "Alpha-5"),
            new("Hazmat spill",                  "Hazmat",  "Resolved",  "2024-03-22 07:30",  28,   "Charlie-1"),
            new("Structure fire - residential",  "Fire",    "Active",    "2024-03-22 04:15",  null,  "Alpha-3"),          // E6
            new("Medical assist",                "Medical", "Resolved",  "2024-03-23 12:00",  5,    "Alpha-2"),
            new("Multi-vehicle collision",       "Rescue",  "Resolved",  "2024-03-23 17:45",  22,   "Bravo-1"),
            new("Gas leak",                      "Hazmat",  "Resolved",  "2024-03-24 10:20",  31,   "Delta-1"),
            new("Kitchen fire",                  "Fire",    "Resolved",  "2024-03-24 15:45",  9,    "Alpha-3"),
            new("Breathing difficulty",          "Medical", "Active",    "2024-03-25 07:30",  null,  "Alpha-4"),          // E6
            new("Vehicle accident - highway",    "Rescue",  "Resolved",  "2024-03-25 11:00",  17,   "Bravo-3"),
            new("Apartment fire",                "Fire",    "Resolved",  "2024-03-26 02:30",  21,   "Alpha-1"),
            new("Chest pain",                    "Medical", "Resolved",  "2024-03-26 09:15",  6,    "Alpha-2"),
            new("Chemical spill",                "Hazmat",  "Resolved",  "2024-03-27 13:40",  42,   "Charlie-2"),
            new("Stroke",                        "Medical", "Resolved",  "2024-03-27 17:55",  9,    "Alpha-3"),
            new("Cliff rescue",                  "Rescue",  "Resolved",  "2024-03-28 14:10",  48,   "Bravo-2"),
            new("Natural gas odor",              "Hazmat",  "Resolved",  "2024-03-28 08:25",  22,   "Delta-2"),
            new("Diabetic emergency",            "Medical", "Active",    "2024-03-29 11:30",  null,  "Alpha-1"),          // E6
            new("Vehicle fire",                  "Fire",    "Resolved",  "2024-03-29 16:50",  10,   "Alpha-5"),
            new("Overdose",                      "Medical", "Resolved",  "2024-03-30 21:45",  13,   "Alpha-3"),
            new("Fuel spill",                    "Hazmat",  "Resolved",  "2024-03-30 09:00",  29,   "Delta-1"),
            new("Medical assist",                "Medical", "Resolved",  "2024-03-31 08:45",  6,    "Alpha-2"),
            new("Structure fire - commercial",   "Fire",    "Resolved",  "2024-03-31 13:20",  24,   "Alpha-4"),
            new("Water rescue",                  "Rescue",  "Active",    "2024-04-01 09:30",  null,  "Bravo-1"),          // E6
            new("Allergic reaction",             "Medical", "Resolved",  "2024-04-01 14:15",  8,    "Alpha-3"),

            // ── Rows 61–90: generated — existing malformed patterns (E1–E6) ──────────────────────────────────
            new("cardiac arrest",                "medical", "resolved",  "04/02/2024 07:45",  9,    "alpha-1"),           // E1
            new("Gas leak",                      "Hazmat",  "RESOLVED",  "2024-04-02",        18,   "Delta-1"),           // E2 E3
            new("vehicle accident - intersection","rescue",  "active",    "04/03/2024 08:15",  null,  "bravo-2"),          // E1 E6
            new("kitchen fire",                  "fire",    "RESOLVED",  "2024-04-03",        11,   "Alpha 2"),           // E1 E2 E3 E5
            new("Breathing difficulty",          "Medical", "resolved",  "04/04/2024 06:55",  999,  "Alpha-3"),           // E1 E4
            new("brush fire",                    "fire",    "active",    "2024-04-04 14:00",  null,  "charlie-1"),         // E1 E6
            new("Elevator rescue",               "Rescue",  "RESOLVED",  "2024-04-05",        33,   "bravo-3"),           // E2 E3 E1(crew)
            new("chemical spill",                "hazmat",  "resolved",  "04/05/2024 16:45",  999,  "charlie-2"),          // E1 E4
            new("Fall injury",                   "Medical", "Resolved",  "2024-04-06 08:10",  8,    null),                // E6
            new("Natural gas odor",              "Hazmat",  "ACTIVE",    "2024-04-06",        null,  "Delta 2"),           // E2 E3 E5 E6
            new("apartment fire",                "fire",    "resolved",  "04/07/2024 03:22",  15,   "alpha-1"),            // E1
            new("Chest pain",                    "Medical", "Resolved",  "2024-04-07 11:40",  6,    null),                // E6
            new("water rescue",                  "rescue",  "resolved",  "04/08/2024 14:20",  27,   "bravo-1"),            // E1
            new("fuel spill",                    "hazmat",  "resolved",  "2024-04-08",        35,   "delta-1"),            // E1 E3
            new("Stroke",                        "Medical", "ACTIVE",    "2024-04-09",        null,  "Alpha 2"),           // E2 E3 E5 E6
            new("vehicle fire",                  "fire",    "resolved",  "04/09/2024 17:15",  13,   "alpha-5"),            // E1
            new("overdose",                      "medical", "resolved",  "2024-04-10 22:45",  10,   "alpha-3"),            // E1
            new("Vehicle extrication",           "Rescue",  "RESOLVED",  "2024-04-10",        40,   "Bravo 2"),            // E2 E3 E5
            new("Warehouse fire",                "Fire",    "Active",    "2024-04-11 02:10",  null,  null),                // E6
            new("diabetic emergency",            "medical", "resolved",  "04/11/2024 14:55",  999,  "alpha-1"),            // E1 E4
            new("mercury exposure",              "hazmat",  "resolved",  "04/12/2024 15:45",  38,   "charlie-2"),           // E1
            new("Allergic reaction",             "Medical", "Resolved",  "2024-04-12",        9,    null),                // E3 E6
            new("dumpster fire",                 "fire",    "resolved",  "04/13/2024 20:30",  6,    "alpha-2"),            // E1
            new("pedestrian struck",             "rescue",  "resolved",  "2024-04-13 16:20",  14,   "bravo-1"),            // E1
            new("Pesticide spill",               "Hazmat",  "ACTIVE",    "2024-04-14",        null,  "Delta 2"),           // E2 E3 E5 E6
            new("unconscious person",            "medical", "resolved",  "04/14/2024 23:10",  11,   "alpha-3"),            // E1
            new("Electrical fire",               "Fire",    "Resolved",  "2024-04-14",        19,   null),                // E3 E6
            new("trench rescue",                 "rescue",  "resolved",  "04/15/2024 10:45",  65,   "bravo-2"),            // E1
            new("cardiac arrest",                "medical", "resolved",  "2024-04-15 18:25",  999,  "alpha-5"),            // E1 E4
            new("Hazmat spill",                  "Hazmat",  "RESOLVED",  "2024-04-16",        28,   "Charlie 1"),          // E2 E3 E5

            // ── Rows 91–110: generated — N1: ISO 8601 T-separator timestamps ──────────────────────────────────
            new("Medical assist",                "Medical", "Resolved",  "2024-04-17T08:30",      6,    "Alpha-2"),
            new("Structure fire - residential",  "Fire",    "Active",    "2024-04-17T04:15:00",   null,  "Alpha-3"),       // E6
            new("Gas leak",                      "Hazmat",  "Resolved",  "2024-04-18T10:20:00",   31,   "Delta-1"),
            new("Cardiac arrest",                "Medical", "Resolved",  "2024-04-18T07:45",      9,    "Alpha-1"),
            new("Vehicle accident - highway",    "Rescue",  "Resolved",  "2024-04-19T11:00:00",   17,   "Bravo-3"),
            new("Kitchen fire",                  "Fire",    "Resolved",  "2024-04-19T15:45",      9,    "Alpha-3"),
            new("Breathing difficulty",          "Medical", "Active",    "2024-04-20T07:30:00",   null,  "Alpha-4"),       // E6
            new("Chemical spill",                "Hazmat",  "Resolved",  "2024-04-20T13:40",      42,   "Charlie-2"),
            new("Stroke",                        "Medical", "Resolved",  "2024-04-21T17:55:00",   9,    "Alpha-3"),
            new("Cliff rescue",                  "Rescue",  "Resolved",  "2024-04-21T14:10",      48,   "Bravo-2"),
            new("Natural gas odor",              "Hazmat",  "Resolved",  "2024-04-22T08:25:00",   22,   "Delta-2"),
            new("Diabetic emergency",            "Medical", "Active",    "2024-04-22T11:30",      null,  "Alpha-1"),       // E6
            new("Vehicle fire",                  "Fire",    "Resolved",  "2024-04-23T16:50:00",   10,   "Alpha-5"),
            new("Overdose",                      "Medical", "Resolved",  "2024-04-23T21:45",      13,   "Alpha-3"),
            new("Fuel spill",                    "Hazmat",  "Resolved",  "2024-04-24T09:00:00",   29,   "Delta-1"),
            new("Structure fire - commercial",   "Fire",    "Resolved",  "2024-04-24T13:20",      24,   "Alpha-4"),
            new("Water rescue",                  "Rescue",  "Active",    "2024-04-25T09:30:00",   null,  "Bravo-1"),       // E6
            new("Allergic reaction",             "Medical", "Resolved",  "2024-04-25T14:15",      8,    "Alpha-3"),
            new("Dumpster fire",                 "Fire",    "Resolved",  "2024-04-26T20:30:00",   6,    "Alpha-2"),
            new("Pedestrian struck",             "Rescue",  "Resolved",  "2024-04-26T16:20",      14,   "Bravo-1"),

            // ── Rows 111–130: generated — N2: timestamps with seconds ────────────────────────────────────────
            new("Cardiac arrest",                "Medical", "Resolved",  "2024-04-27 07:45:12",   9,    "Alpha-1"),
            new("Gas leak",                      "Hazmat",  "Resolved",  "2024-04-27 12:30:45",   18,   "Delta-1"),
            new("Vehicle accident - intersection","Rescue",  "Active",    "2024-04-28 08:15:33",   null,  "Bravo-2"),      // E6
            new("Kitchen fire",                  "Fire",    "Resolved",  "2024-04-28 19:22:07",   11,   "Alpha-2"),
            new("Breathing difficulty",          "Medical", "Resolved",  "2024-04-29 06:55:28",   7,    "Alpha-3"),
            new("Chemical spill",                "Hazmat",  "Resolved",  "2024-04-29 16:45:51",   45,   "Charlie-2"),
            new("Fall injury",                   "Medical", "Resolved",  "2024-04-30 08:10:03",   8,    "Alpha-4"),
            new("Apartment fire",                "Fire",    "Resolved",  "2024-04-30 03:22:49",   15,   "Alpha-1"),
            new("Water rescue",                  "Rescue",  "Resolved",  "2024-05-01 14:20:37",   27,   "Bravo-1"),
            new("Stroke",                        "Medical", "Active",    "2024-05-01 08:00:15",   null,  "Alpha-2"),       // E6
            new("Vehicle fire",                  "Fire",    "Resolved",  "2024-05-02 17:15:22",   13,   "Alpha-5"),
            new("Overdose",                      "Medical", "Resolved",  "2024-05-02 22:45:08",   10,   "Alpha-3"),
            new("Warehouse fire",                "Fire",    "Active",    "2024-05-03 02:10:54",   null,  null),            // E6
            new("Confined space rescue",         "Rescue",  "Resolved",  "2024-05-03 11:20:41",   52,   "Bravo-3"),
            new("Natural gas odor",              "Hazmat",  "Resolved",  "2024-05-04 08:25:16",   22,   "Delta-2"),
            new("Pesticide spill",               "Hazmat",  "Active",    "2024-05-04 08:00:00",   null,  "Delta-2"),       // E6
            new("Electrical fire",               "Fire",    "Resolved",  "2024-05-05 14:30:39",   19,   "Alpha-1"),
            new("Pedestrian struck",             "Rescue",  "Resolved",  "2024-05-05 16:20:02",   14,   "Bravo-1"),
            new("Diabetic emergency",            "Medical", "Active",    "2024-05-06 11:30:27",   null,  "Alpha-1"),       // E6
            new("Fuel spill",                    "Hazmat",  "Resolved",  "2024-05-06 09:00:43",   29,   "Delta-1"),

            // ── Rows 131–150: generated — N3: leading/trailing whitespace on text fields ─────────────────────
            new("  Medical assist  ",            "  Medical  ",  "  Resolved  ",  "2024-05-07 08:45",  6,    "  Alpha-2  "),
            new(" Structure fire - residential ", " Fire ",      " Active ",      "2024-05-07 04:15",  null,  " Alpha-3 "),  // E6
            new(" Gas leak ",                    " Hazmat ",     " Resolved ",    "2024-05-08 10:20",  31,   " Delta-1 "),
            new("  Cardiac arrest  ",            "  Medical  ",  "  Resolved  ",  "2024-05-08 07:45",  9,    "  Alpha-1  "),
            new(" Vehicle accident - highway ",  " Rescue ",     " Resolved ",    "2024-05-09 11:00",  17,   " Bravo-3 "),
            new(" Kitchen fire ",                " Fire ",       " Resolved ",    "2024-05-09 15:45",  9,    " Alpha-3 "),
            new("  Breathing difficulty  ",      "  Medical  ",  "  Active  ",    "2024-05-10 07:30",  null,  "  Alpha-4  "), // E6
            new(" Chemical spill ",              " Hazmat ",     " Resolved ",    "2024-05-10 13:40",  42,   " Charlie-2 "),
            new("  Stroke  ",                    "  Medical  ",  "  Resolved  ",  "2024-05-11 17:55",  9,    "  Alpha-3  "),
            new(" Cliff rescue ",                " Rescue ",     " Resolved ",    "2024-05-11 14:10",  48,   " Bravo-2 "),
            new("  Natural gas odor  ",          "  Hazmat  ",   "  Resolved  ",  "2024-05-12 08:25",  22,   "  Delta-2  "),
            new(" Diabetic emergency ",          " Medical ",    " Active ",      "2024-05-12 11:30",  null,  " Alpha-1 "),  // E6
            new("  Vehicle fire  ",              "  Fire  ",     "  Resolved  ",  "2024-05-13 16:50",  10,   "  Alpha-5  "),
            new(" Overdose ",                    " Medical ",    " Resolved ",    "2024-05-13 21:45",  13,   " Alpha-3 "),
            new("  Apartment fire  ",            "  Fire  ",     "  Resolved  ",  "2024-05-14 03:22",  15,   "  Alpha-1  "),
            new(" Chest pain ",                  " Medical ",    " Resolved ",    "2024-05-14 11:40",  6,    " Alpha-3 "),
            new("  Water rescue  ",              "  Rescue  ",   "  Resolved  ",  "2024-05-15 14:20",  27,   "  Bravo-1  "),
            new(" Fuel spill ",                  " Hazmat ",     " Resolved ",    "2024-05-15 09:30",  35,   " Delta-1 "),
            new("  Unconscious person  ",        "  Medical  ",  "  Resolved  ",  "2024-05-16 23:10",  11,   "  Alpha-3  "),
            new(" Electrical fire ",             " Fire ",       " Resolved ",    "2024-05-16 14:30",  19,   " Alpha-1 "),

            // ── Rows 151–170: generated — N4: ALL-CAPS category ──────────────────────────────────────────────
            new("Medical assist",                "MEDICAL", "Resolved",  "2024-05-17 08:45",  6,    "Alpha-2"),
            new("Structure fire - residential",  "FIRE",    "Active",    "2024-05-17 04:15",  null,  "Alpha-3"),           // E6
            new("Gas leak",                      "HAZMAT",  "Resolved",  "2024-05-18 10:20",  31,   "Delta-1"),
            new("Vehicle accident",              "RESCUE",  "Resolved",  "2024-05-18 11:00",  17,   "Bravo-3"),
            new("Cardiac arrest",                "MEDICAL", "RESOLVED",  "2024-05-19 07:45",  9,    "Alpha-1"),            // E2
            new("Kitchen fire",                  "FIRE",    "RESOLVED",  "2024-05-19 15:45",  9,    "Alpha-3"),            // E2
            new("Breathing difficulty",          "MEDICAL", "Active",    "2024-05-20 07:30",  null,  "Alpha-4"),           // E6
            new("Chemical spill",                "HAZMAT",  "Resolved",  "2024-05-20 13:40",  42,   "Charlie-2"),
            new("Cliff rescue",                  "RESCUE",  "Resolved",  "2024-05-21 14:10",  48,   "Bravo-2"),
            new("Natural gas odor",              "HAZMAT",  "ACTIVE",    "2024-05-21 08:25",  null,  "Delta-2"),           // E2 E6
            new("Diabetic emergency",            "MEDICAL", "Active",    "2024-05-22 11:30",  null,  "Alpha-1"),           // E6
            new("Vehicle fire",                  "FIRE",    "Resolved",  "2024-05-22 16:50",  10,   "Alpha-5"),
            new("Overdose",                      "MEDICAL", "RESOLVED",  "2024-05-23 21:45",  13,   "Alpha-3"),            // E2
            new("Apartment fire",                "FIRE",    "Active",    "2024-05-23 03:22",  null,  null),                // E6
            new("Chest pain",                    "MEDICAL", "Resolved",  "2024-05-24 11:40",  6,    "Alpha-3"),
            new("Water rescue",                  "RESCUE",  "Active",    "2024-05-24 09:30",  null,  "Bravo-1"),           // E6
            new("Fuel spill",                    "HAZMAT",  "Resolved",  "2024-05-25 09:00",  29,   "Delta-1"),
            new("Unconscious person",            "MEDICAL", "Resolved",  "2024-05-25 23:10",  11,   "Alpha-3"),
            new("Electrical fire",               "FIRE",    "Resolved",  "2024-05-26 14:30",  19,   "Alpha-1"),
            new("Trench rescue",                 "RESCUE",  "Resolved",  "2024-05-26 10:45",  65,   "Bravo-2"),

            // ── Rows 171–190: generated — N5: slash-separated crew identifier ─────────────────────────────────
            new("Medical assist",                "Medical", "Resolved",  "2024-05-27 08:45",  6,    "Alpha/2"),
            new("Structure fire - residential",  "Fire",    "Active",    "2024-05-27 04:15",  null,  "Alpha/3"),           // E6
            new("Gas leak",                      "Hazmat",  "Resolved",  "2024-05-28 10:20",  31,   "Delta/1"),
            new("Cardiac arrest",                "Medical", "Resolved",  "2024-05-28 07:45",  9,    "alpha/1"),            // N5 E1(crew)
            new("Vehicle accident - highway",    "Rescue",  "Resolved",  "2024-05-29 11:00",  17,   "Bravo/3"),
            new("Kitchen fire",                  "Fire",    "Resolved",  "2024-05-29 15:45",  9,    "Alpha/3"),
            new("Breathing difficulty",          "Medical", "Active",    "2024-05-30 07:30",  null,  "alpha/4"),           // E6 N5 E1(crew)
            new("Chemical spill",                "Hazmat",  "Resolved",  "2024-05-30 13:40",  42,   "Charlie/2"),
            new("Stroke",                        "Medical", "Resolved",  "2024-05-31 17:55",  9,    "Alpha/3"),
            new("Cliff rescue",                  "Rescue",  "Resolved",  "2024-05-31 14:10",  48,   "bravo/2"),            // N5 E1(crew)
            new("Natural gas odor",              "Hazmat",  "Resolved",  "2024-06-01 08:25",  22,   "Delta/2"),
            new("Diabetic emergency",            "Medical", "Active",    "2024-06-01 11:30",  null,  "alpha/1"),           // E6 N5 E1(crew)
            new("Vehicle fire",                  "Fire",    "Resolved",  "2024-06-02 16:50",  10,   "Alpha/5"),
            new("Overdose",                      "Medical", "Resolved",  "2024-06-02 21:45",  13,   "alpha/3"),            // N5 E1(crew)
            new("Apartment fire",                "Fire",    "Resolved",  "2024-06-03 03:22",  15,   "Alpha/1"),
            new("Chest pain",                    "Medical", "Resolved",  "2024-06-03 11:40",  6,    "Alpha/3"),
            new("Water rescue",                  "Rescue",  "Active",    "2024-06-04 09:30",  null,  "Bravo/1"),           // E6
            new("Fuel spill",                    "Hazmat",  "Resolved",  "2024-06-04 09:00",  29,   "delta/1"),            // N5 E1(crew)
            new("Electrical fire",               "Fire",    "Resolved",  "2024-06-05 14:30",  19,   "Alpha/1"),
            new("Trench rescue",                 "Rescue",  "Resolved",  "2024-06-05 10:45",  65,   "bravo/2"),            // N5 E1(crew)

            // ── Rows 191–200: generated — combined malformed patterns ─────────────────────────────────────────
            new("  cardiac arrest  ",  "  MEDICAL  ",  "  RESOLVED  ",  "2024-06-06T07:45:12",  9,    "  alpha/1  "),     // N3 N4 E1 E2 N1 N5
            new("gas leak",            "HAZMAT",       "resolved",       "2024-06-06 12:30:45",  999,  "Delta/1"),          // E1 N4 N2 E4 N5
            new(" vehicle accident ",  "rescue",       "ACTIVE",         "2024-06-07T08:15",     null,  "bravo/2"),         // N3 E1 E2 N1 E6 N5
            new("kitchen fire",        "FIRE",         "resolved",       "2024-06-07 19:22:07",  11,   "  alpha 2  "),      // E1 N4 N2 N3 E5
            new("  Breathing difficulty  ", "medical", "RESOLVED",       "2024-06-08T06:55:00",  7,    "  Alpha/4  "),      // N3 E1 E2 N1 N5
            new("chemical spill",      "HAZMAT",       "RESOLVED",       "2024-06-08 16:45:51",  999,  "charlie/2"),        // E1 N4 E2 N2 E4 N5 E1(crew)
            new("  Fall injury  ",     "  medical  ",  "  Active  ",     "2024-06-09T08:10:03",  null,  null),              // N3 E1 N1 E6
            new("apartment fire",      "fire",         "RESOLVED",       "2024-06-09 03:22:49",  15,   "  alpha/1  "),      // E1 E2 N2 N3 N5 E1(crew)
            new("water rescue",        "RESCUE",       "resolved",       "2024-06-10T14:20:37",  27,   "  bravo/1  "),      // E1 N4 N1 N3 N5
            new("  Stroke  ",          "  MEDICAL  ",  "  active  ",     "2024-06-10 08:00:15",  null,  "  Alpha/2  "),     // N3 N4 E1 N2 E6 N5
        ];

        public static void Seed(IncidentDbContext db)
        {
            if (db.Incidents.Count() >= 200) return;

            db.Incidents.RemoveRange(db.Incidents.ToList());
            db.SaveChanges();
            db.Incidents.AddRange(SourceRows.Select(IncidentNormalizer.Normalize));
            db.SaveChanges();
        }
    }
}
