using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events (public)
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events.Select(e => new EventDto
            {
                EventID = e.EventID,
                EventName = e.EventName,
                EventType = e.EventType,
                EventDate = e.EventDate,
                Location = e.Location,
                Description = e.Description,
                OrganiserName = e.Organiser?.FullName ?? "",
                Categories = e.Categories.Select(c => new CategoryDto
                {
                    CategoryID = c.CategoryID,
                    EventID = c.EventID,
                    CategoryName = c.CategoryName,
                    Distance = c.Distance,
                    MaxParticipants = c.MaxParticipants,
                    Price = c.Price
                }).ToList()
            }));
        }

        // GET: api/events/{id} (public)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var e = await _context.Events
                .Include(ev => ev.Organiser)
                .Include(ev => ev.Categories)
                .FirstOrDefaultAsync(ev => ev.EventID == id);

            if (e == null) return NotFound();

            return Ok(new EventDto
            {
                EventID = e.EventID,
                EventName = e.EventName,
                EventType = e.EventType,
                EventDate = e.EventDate,
                Location = e.Location,
                Description = e.Description,
                OrganiserName = e.Organiser?.FullName ?? "",
                Categories = e.Categories.Select(c => new CategoryDto
                {
                    CategoryID = c.CategoryID,
                    EventID = c.EventID,
                    CategoryName = c.CategoryName,
                    Distance = c.Distance,
                    MaxParticipants = c.MaxParticipants,
                    Price = c.Price
                }).ToList()
            });
        }

        // POST: api/events (Organiser only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (!new[] { "Running", "Walking", "Cycling" }.Contains(dto.EventType))
                return BadRequest(new { message = "EventType must be Running, Walking, or Cycling." });

            var ev = new Event
            {
                OrganiserID = organiserId,
                EventName = dto.EventName,
                EventType = dto.EventType,
                EventDate = dto.EventDate,
                Location = dto.Location,
                Description = dto.Description
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = ev.EventID }, new { id = ev.EventID, message = "Event created." });
        }

        // PUT: api/events/{id} (Organiser only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (ev.OrganiserID != organiserId) return Forbidden();

            if (!string.IsNullOrEmpty(dto.EventName)) ev.EventName = dto.EventName;
            if (!string.IsNullOrEmpty(dto.EventType))
            {
                if (!new[] { "Running", "Walking", "Cycling" }.Contains(dto.EventType))
                    return BadRequest(new { message = "Invalid EventType." });
                ev.EventType = dto.EventType;
            }
            if (dto.EventDate.HasValue) ev.EventDate = dto.EventDate.Value;
            if (!string.IsNullOrEmpty(dto.Location)) ev.Location = dto.Location;
            if (dto.Description != null) ev.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated." });
        }

        // DELETE: api/events/{id} (Organiser only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (ev.OrganiserID != organiserId) return Forbidden();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private IActionResult Forbidden()
        {
            return StatusCode(403, new { message = "You do not own this event." });
        }
    }
}