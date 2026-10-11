using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthControllerTests
    {
        private RaceDayDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        private ITokenService GetTokenService()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "TestKeyForUnitTestingMustBeAtLeast256BitsLongForHS256!" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" }
                })
                .Build();
            return new TokenService(config);
        }

        [Fact]
        public async Task Register_Organiser_ReturnsCreated()
        {
            var context = GetInMemoryContext();
            var controller = new AuthController(context, GetTokenService());

            var dto = new RegisterDto
            {
                FullName = "Test Organiser",
                Email = "org@test.com",
                Password = "Password123",
                Role = "Organiser"
            };

            var result = await controller.Register(dto);

            result.Should().BeOfType<CreatedAtActionResult>();
            context.Organisers.Count().Should().Be(1);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            var context = GetInMemoryContext();
            var controller = new AuthController(context, GetTokenService());

            var dto = new RegisterDto
            {
                FullName = "Test",
                Email = "dup@test.com",
                Password = "Password123",
                Role = "Participant"
            };

            await controller.Register(dto);
            var result = await controller.Register(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            var context = GetInMemoryContext();
            var controller = new AuthController(context, GetTokenService());

            await controller.Register(new RegisterDto
            {
                FullName = "Login Test",
                Email = "login@test.com",
                Password = "Password123",
                Role = "Organiser"
            });

            var result = await controller.Login(new LoginDto
            {
                Email = "login@test.com",
                Password = "Password123",
                Role = "Organiser"
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var context = GetInMemoryContext();
            var controller = new AuthController(context, GetTokenService());

            await controller.Register(new RegisterDto
            {
                FullName = "Login Test",
                Email = "login@test.com",
                Password = "Password123",
                Role = "Organiser"
            });

            var result = await controller.Login(new LoginDto
            {
                Email = "login@test.com",
                Password = "WrongPassword",
                Role = "Organiser"
            });

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}