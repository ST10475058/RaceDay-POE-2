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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events/{eventId}/categories (public)
        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists) return NotFound();

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .Include(c => c.Enrolments)
                .ToListAsync();

            return Ok(categories.Select(c => new CategoryDto
            {
                CategoryID = c.CategoryID,
                EventID = c.EventID,
                CategoryName = c.CategoryName,
                Distance = c.Distance,
                MaxParticipants = c.MaxParticipants,
                Price = c.Price,
                CurrentEnrolments = c.Enrolments.Count(e => e.Status == "Confirmed")
            }));
        }

        // POST: api/events/{eventId}/categories (Organiser only)
        [HttpPost("events/{eventId}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> AddCategory(int eventId, [FromBody] CreateCategoryDto dto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });

            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (ev.OrganiserID != organiserId) return StatusCode(403, new { message = "Not your event." });

            if (dto.Distance <= 0) return BadRequest(new { message = "Distance must be > 0." });
            if (dto.MaxParticipants <= 0) return BadRequest(new { message = "MaxParticipants must be > 0." });

            var category = new Category
            {
                EventID = eventId,
                CategoryName = dto.CategoryName,
                Distance = dto.Distance,
                MaxParticipants = dto.MaxParticipants,
                Price = dto.Price
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), new { eventId }, new { id = category.CategoryID, message = "Category created." });
        }

        // PUT: api/categories/{id} (Organiser only)
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (category.Event?.OrganiserID != organiserId) return StatusCode(403, new { message = "Not your event." });

            if (!string.IsNullOrEmpty(dto.CategoryName)) category.CategoryName = dto.CategoryName;
            if (dto.Distance.HasValue && dto.Distance.Value > 0) category.Distance = dto.Distance.Value;
            if (dto.MaxParticipants.HasValue && dto.MaxParticipants.Value > 0) category.MaxParticipants = dto.MaxParticipants.Value;
            if (dto.Price.HasValue) category.Price = dto.Price.Value;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Category updated." });
        }

        // DELETE: api/categories/{id} (Organiser only)
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (category.Event?.OrganiserID != organiserId) return StatusCode(403, new { message = "Not your event." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}