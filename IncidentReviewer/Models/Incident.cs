using System.ComponentModel.DataAnnotations.Schema;

namespace IncidentReviewer.Models
{
    public enum IncidentStatus { Active, Resolved }

    public class Incident
    {
        public const int OutlierThresholdMinutes = 300;

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public IncidentStatus Status { get; set; } = IncidentStatus.Active;
        public string? AssignedCrew { get; set; }
        public int? ResponseTimeMinutes { get; set; }

        [NotMapped]
        public bool IsResponseTimeOutlier => ResponseTimeMinutes > OutlierThresholdMinutes;
    }

    public record CreateIncidentDto(
        string Title,
        string Category,
        DateTime Timestamp,
        string? AssignedCrew,
        int? ResponseTimeMinutes);

    public record UpdateStatusDto(IncidentStatus Status);
}
