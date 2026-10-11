using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentsControllerTests
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
        public async Task Enrol_AsParticipant_ReturnsCreated()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event { EventID = 1, OrganiserID = 1, EventName = "E", EventType = "Running", EventDate = DateTime.Now, Location = "L" });
            context.Categories.Add(new Category { CategoryID = 1, EventID = 1, CategoryName = "5km", Distance = 5, MaxParticipants = 100, Price = 0 });
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context);
            SetUserContext(controller, "Participant", 1);

            var result = await controller.Enrol(1);

            result.Should().BeOfType<CreatedAtActionResult>();
            context.Enrolments.Count().Should().Be(1);
        }

        [Fact]
        public async Task Enrol_AlreadyEnrolled_ReturnsConflict()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event { EventID = 1, OrganiserID = 1, EventName = "E", EventType = "Running", EventDate = DateTime.Now, Location = "L" });
            context.Categories.Add(new Category { CategoryID = 1, EventID = 1, CategoryName = "5km", Distance = 5, MaxParticipants = 100, Price = 0 });
            context.Participants.Add(new Participant { ParticipantID = 1, FullName = "P", Email = "p@test.com", PasswordHash = "x" });
            context.Enrolments.Add(new Enrolment { EnrolmentID = 1, ParticipantID = 1, CategoryID = 1, Status = "Confirmed" });
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context);
            SetUserContext(controller, "Participant", 1);

            var result = await controller.Enrol(1);

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Enrol_CategoryFull_ReturnsBadRequest()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event { EventID = 1, OrganiserID = 1, EventName = "E", EventType = "Running", EventDate = DateTime.Now, Location = "L" });
            context.Categories.Add(new Category { CategoryID = 1, EventID = 1, CategoryName = "5km", Distance = 5, MaxParticipants = 1, Price = 0 });
            context.Participants.Add(new Participant { ParticipantID = 1, FullName = "P1", Email = "p1@test.com", PasswordHash = "x" });
            context.Participants.Add(new Participant { ParticipantID = 2, FullName = "P2", Email = "p2@test.com", PasswordHash = "x" });
            context.Enrolments.Add(new Enrolment { EnrolmentID = 1, ParticipantID = 1, CategoryID = 1, Status = "Confirmed" });
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context);
            SetUserContext(controller, "Participant", 2);

            var result = await controller.Enrol(1);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}