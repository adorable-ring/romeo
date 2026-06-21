using IncidentReviewer.Data;
using IncidentReviewer.Models;
using IncidentReviewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IncidentReviewer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IncidentsController(IncidentDbContext db) : ControllerBase
    {
        /// <summary>Returns incidents, optionally filtered.</summary>
        /// <param name="category">Category name (e.g. Fire, Medical, Traffic)</param>
        /// <param name="status">Incident status</param>
        /// <param name="from">Start date — format: YYYY-MM-DD</param>
        /// <param name="to">End date (inclusive) — format: YYYY-MM-DD</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Incident>>> Get(
            [FromQuery] string? category,
            [FromQuery] IncidentStatus? status,
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to)
        {
            var query = db.Incidents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(i => i.Category == IncidentNormalizer.NormalizeWord(category));

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            if (from.HasValue)
                query = query.Where(i => i.Timestamp >= from.Value.ToDateTime(TimeOnly.MinValue));

            if (to.HasValue)
                query = query.Where(i => i.Timestamp <= to.Value.ToDateTime(TimeOnly.MaxValue));

            return Ok(await query.OrderByDescending(i => i.Timestamp).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Incident>> GetById(int id)
        {
            var incident = await db.Incidents.FindAsync(id);
            return incident is null ? NotFound() : Ok(incident);
        }

        [HttpPost]
        public async Task<ActionResult<Incident>> Create(CreateIncidentDto dto)
        {
            var incident = Map(dto);
            db.Incidents.Add(incident);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
        }

        [HttpPost("batch")]
        public async Task<ActionResult<IEnumerable<Incident>>> CreateBatch(IEnumerable<CreateIncidentDto> dtos)
        {
            var incidents = dtos.Select(Map).ToList();
            db.Incidents.AddRange(incidents);
            await db.SaveChangesAsync();
            return Ok(incidents);
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<Incident>> UpdateStatus(int id, UpdateStatusDto dto)
        {
            var incident = await db.Incidents.FindAsync(id);
            if (incident is null) return NotFound();
            incident.Status = dto.Status;
            await db.SaveChangesAsync();
            return Ok(incident);
        }

        private static Incident Map(CreateIncidentDto dto) => new()
        {
            Title = IncidentNormalizer.NormalizeTitle(dto.Title),
            Category = IncidentNormalizer.NormalizeWord(dto.Category),
            Timestamp = dto.Timestamp,
            Status = IncidentStatus.Active,
            AssignedCrew = IncidentNormalizer.NormalizeCrew(dto.AssignedCrew),
            ResponseTimeMinutes = dto.ResponseTimeMinutes,
        };
    }
}
