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
    [Route("api")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/categories/{categoryId}/enrolments (Participant only)
        [HttpPost("categories/{categoryId}/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrol(int categoryId)
        {
            var participantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var category = await _context.Categories
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryID == categoryId);

            if (category == null) return NotFound(new { message = "Category not found." });

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.ParticipantID == participantId && e.CategoryID == categoryId);

            if (alreadyEnrolled) return Conflict(new { message = "Already enrolled in this category." });

            var confirmedCount = category.Enrolments.Count(e => e.Status == "Confirmed");
            if (confirmedCount >= category.MaxParticipants)
                return BadRequest(new { message = "Category is full." });

            var enrolment = new Enrolment
            {
                ParticipantID = participantId,
                CategoryID = categoryId,
                Status = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyEnrolments), null, new { id = enrolment.EnrolmentID, message = "Enrolled successfully." });
        }

        // GET: api/enrolments/me (Participant only)
        [HttpGet("enrolments/me")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var participantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantID == participantId)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                    .ThenInclude(c => c!.Event)
                .ToListAsync();

            return Ok(enrolments.Select(e => new EnrolmentDto
            {
                EnrolmentID = e.EnrolmentID,
                ParticipantID = e.ParticipantID,
                ParticipantName = e.Participant?.FullName ?? "",
                CategoryID = e.CategoryID,
                CategoryName = e.Category?.CategoryName ?? "",
                Distance = e.Category?.Distance ?? 0,
                Price = e.Category?.Price ?? 0,
                EventName = e.Category?.Event?.EventName ?? "",
                EventDate = e.Category?.Event?.EventDate ?? DateTime.MinValue,
                EnrolmentDate = e.EnrolmentDate,
                Status = e.Status
            }));
        }

        // GET: api/events/{eventId}/enrolments (Organiser only)
        [HttpGet("events/{eventId}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserID != organiserId) return StatusCode(403, new { message = "Not your event." });

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                    .ThenInclude(c => c!.Event)
                .Where(e => e.Category!.EventID == eventId)
                .ToListAsync();

            return Ok(enrolments.Select(e => new EnrolmentDto
            {
                EnrolmentID = e.EnrolmentID,
                ParticipantID = e.ParticipantID,
                ParticipantName = e.Participant?.FullName ?? "",
                CategoryID = e.CategoryID,
                CategoryName = e.Category?.CategoryName ?? "",
                Distance = e.Category?.Distance ?? 0,
                Price = e.Category?.Price ?? 0,
                EventName = e.Category?.Event?.EventName ?? "",
                EventDate = e.Category?.Event?.EventDate ?? DateTime.MinValue,
                EnrolmentDate = e.EnrolmentDate,
                Status = e.Status
            }));
        }
    }
}