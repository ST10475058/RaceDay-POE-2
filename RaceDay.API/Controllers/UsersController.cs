using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/users/me
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (role == "Organiser")
            {
                var organiser = await _context.Organisers.FindAsync(userId);
                if (organiser == null) return NotFound();

                return Ok(new UserProfileDto
                {
                    Id = organiser.OrganiserID,
                    FullName = organiser.FullName,
                    Email = organiser.Email,
                    PhoneNumber = organiser.PhoneNumber,
                    Role = "Organiser",
                    CreatedAt = organiser.CreatedAt
                });
            }
            else
            {
                var participant = await _context.Participants.FindAsync(userId);
                if (participant == null) return NotFound();

                return Ok(new UserProfileDto
                {
                    Id = participant.ParticipantID,
                    FullName = participant.FullName,
                    Email = participant.Email,
                    PhoneNumber = participant.PhoneNumber,
                    DateOfBirth = participant.DateOfBirth,
                    Role = "Participant",
                    CreatedAt = participant.CreatedAt
                });
            }
        }

        // PUT: api/users/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (role == "Organiser")
            {
                var organiser = await _context.Organisers.FindAsync(userId);
                if (organiser == null) return NotFound();

                if (!string.IsNullOrEmpty(dto.FullName)) organiser.FullName = dto.FullName;
                if (dto.PhoneNumber != null) organiser.PhoneNumber = dto.PhoneNumber;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Profile updated." });
            }
            else
            {
                var participant = await _context.Participants.FindAsync(userId);
                if (participant == null) return NotFound();

                if (!string.IsNullOrEmpty(dto.FullName)) participant.FullName = dto.FullName;
                if (dto.PhoneNumber != null) participant.PhoneNumber = dto.PhoneNumber;
                if (dto.DateOfBirth.HasValue) participant.DateOfBirth = dto.DateOfBirth;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Profile updated." });
            }
        }
    }
}