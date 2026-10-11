using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;
using Xunit;

namespace RaceDay.Tests
{
    public class EventsControllerTests
    {
        private RaceDayDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        private void SetUserContext(ControllerBase controller, string role, int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {
            var context = GetInMemoryContext();
            var controller = new EventsController(context);
            SetUserContext(controller, "Organiser", 1);

            var dto = new CreateEventDto
            {
                EventName = "Test Race",
                EventType = "Running",
                EventDate = DateTime.Now.AddMonths(1),
                Location = "Test Location"
            };

            var result = await controller.CreateEvent(dto);

            result.Should().BeOfType<CreatedAtActionResult>();
            context.Events.Count().Should().Be(1);
        }

        [Fact]
        public async Task UpdateEvent_NotOwner_ReturnsForbidden()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event
            {
                EventID = 1,
                OrganiserID = 99,
                EventName = "Test",
                EventType = "Running",
                EventDate = DateTime.Now,
                Location = "Test"
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            SetUserContext(controller, "Organiser", 1); // Different ID

            var result = await controller.UpdateEvent(1, new UpdateEventDto { EventName = "Updated" });

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task DeleteEvent_NotOwner_ReturnsForbidden()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event
            {
                EventID = 1,
                OrganiserID = 99,
                EventName = "Test",
                EventType = "Running",
                EventDate = DateTime.Now,
                Location = "Test"
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            SetUserContext(controller, "Organiser", 1);

            var result = await controller.DeleteEvent(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(403);
        }
    }
}