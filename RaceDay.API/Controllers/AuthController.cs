using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(RaceDayDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto.Role != "Organiser" && dto.Role != "Participant")
                return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'." });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            if (dto.Role == "Organiser")
            {
                if (await _context.Organisers.AnyAsync(o => o.Email == dto.Email))
                    return BadRequest(new { message = "Email already registered." });

                var organiser = new Organiser
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                    PhoneNumber = dto.PhoneNumber
                };

                _context.Organisers.Add(organiser);
                await _context.SaveChangesAsync();

                var token = _tokenService.CreateToken(organiser.Email, "Organiser", organiser.OrganiserID);

                return CreatedAtAction(nameof(Register), new AuthResponseDto
                {
                    Token = token,
                    Role = "Organiser",
                    UserId = organiser.OrganiserID,
                    FullName = organiser.FullName,
                    Email = organiser.Email
                });
            }
            else
            {
                if (await _context.Participants.AnyAsync(p => p.Email == dto.Email))
                    return BadRequest(new { message = "Email already registered." });

                var participant = new Participant
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                    DateOfBirth = dto.DateOfBirth,
                    PhoneNumber = dto.PhoneNumber
                };

                _context.Participants.Add(participant);
                await _context.SaveChangesAsync();

                var token = _tokenService.CreateToken(participant.Email, "Participant", participant.ParticipantID);

                return CreatedAtAction(nameof(Register), new AuthResponseDto
                {
                    Token = token,
                    Role = "Participant",
                    UserId = participant.ParticipantID,
                    FullName = participant.FullName,
                    Email = participant.Email
                });
            }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto.Role == "Organiser")
            {
                var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.Email == dto.Email);
                if (organiser == null || !BCrypt.Net.BCrypt.Verify(dto.Password, organiser.PasswordHash))
                    return Unauthorized(new { message = "Invalid credentials." });

                var token = _tokenService.CreateToken(organiser.Email, "Organiser", organiser.OrganiserID);

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    Role = "Organiser",
                    UserId = organiser.OrganiserID,
                    FullName = organiser.FullName,
                    Email = organiser.Email
                });
            }
            else if (dto.Role == "Participant")
            {
                var participant = await _context.Participants.FirstOrDefaultAsync(p => p.Email == dto.Email);
                if (participant == null || !BCrypt.Net.BCrypt.Verify(dto.Password, participant.PasswordHash))
                    return Unauthorized(new { message = "Invalid credentials." });

                var token = _tokenService.CreateToken(participant.Email, "Participant", participant.ParticipantID);

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    Role = "Participant",
                    UserId = participant.ParticipantID,
                    FullName = participant.FullName,
                    Email = participant.Email
                });
            }

            return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'." });
        }
    }
}