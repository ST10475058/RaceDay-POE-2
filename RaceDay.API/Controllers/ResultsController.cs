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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/enrolments/{enrolmentId}/results (Organiser only)
        [HttpPost("enrolments/{enrolmentId}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> RecordResult(int enrolmentId, [FromBody] CreateResultDto dto)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                    .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.Category?.Event?.OrganiserID != organiserId)
                return StatusCode(403, new { message = "Not your event." });

            var exists = await _context.Results.AnyAsync(r => r.EnrolmentID == enrolmentId);
            if (exists) return Conflict(new { message = "Result already captured for this enrolment." });

            if (dto.Position.HasValue && dto.Position.Value <= 0)
                return BadRequest(new { message = "Position must be > 0." });

            var result = new Result
            {
                EnrolmentID = enrolmentId,
                RecordedByOrganiserID = organiserId,
                FinishTime = dto.FinishTime,
                Position = dto.Position
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventResults),
                new { eventId = enrolment.Category?.EventID },
                new { id = result.ResultID, message = "Result recorded." });
        }

        // GET: api/results/me (Participant only)
        [HttpGet("results/me")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var participantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                        .ThenInclude(c => c!.Event)
                .Where(r => r.Enrolment!.ParticipantID == participantId)
                .OrderBy(r => r.Position)
                .ToListAsync();

            return Ok(results.Select(r => new ResultDto
            {
                ResultID = r.ResultID,
                EnrolmentID = r.EnrolmentID,
                ParticipantName = r.Enrolment?.Participant?.FullName ?? "",
                CategoryName = r.Enrolment?.Category?.CategoryName ?? "",
                EventName = r.Enrolment?.Category?.Event?.EventName ?? "",
                FinishTime = r.FinishTime,
                Position = r.Position,
                RecordedAt = r.RecordedAt
            }));
        }

        // GET: api/events/{eventId}/results (public)
        [HttpGet("events/{eventId}/results")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                        .ThenInclude(c => c!.Event)
                .Where(r => r.Enrolment!.Category!.EventID == eventId)
                .OrderBy(r => r.Position)
                .ToListAsync();

            return Ok(results.Select(r => new ResultDto
            {
                ResultID = r.ResultID,
                EnrolmentID = r.EnrolmentID,
                ParticipantName = r.Enrolment?.Participant?.FullName ?? "",
                CategoryName = r.Enrolment?.Category?.CategoryName ?? "",
                EventName = r.Enrolment?.Category?.Event?.EventName ?? "",
                FinishTime = r.FinishTime,
                Position = r.Position,
                RecordedAt = r.RecordedAt
            }));
        }
    }
}