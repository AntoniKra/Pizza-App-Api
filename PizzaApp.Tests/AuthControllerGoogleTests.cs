using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PizzaApp.Controllers;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Entities;
using PizzaApp.Services;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PizzaApp.Tests
{
    public class AuthControllerGoogleTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private IConfiguration GetConfig()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "SuperSecretKeyForTesting1234567890!"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };
            return new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        }

        [Fact]
        public async Task GoogleLogin_ValidToken_NewUser_CreatesCustomer()
        {
            var db = GetDbContext();
            var googleMock = new Mock<IGoogleAuthService>();
            googleMock.Setup(g => g.ValidateAsync(It.IsAny<string>()))
                      .ReturnsAsync(new GoogleJsonWebSignature.Payload { Email = "nowy@gmail.com", GivenName = "Nowy" });

            var controller = new AuthController(db, GetConfig(), googleMock.Object);
            var result = await controller.GoogleLogin(new GoogleLoginDto { IdToken = "valid_token" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<LoginResponseDto>(okResult.Value);
            Assert.Equal("nowy@gmail.com", responseDto.Email);
            Assert.False(responseDto.IsOwner);

            var userInDb = await db.Accounts.FirstOrDefaultAsync(u => u.Email == "nowy@gmail.com");
            Assert.NotNull(userInDb);
            Assert.IsType<Customer>(userInDb);
            Assert.Equal("", userInDb.PasswordHash);
        }

        [Fact]
        public async Task GoogleLogin_ValidToken_ExistingOwner_LogsInAsOwner()
        {
            var db = GetDbContext();
            db.Accounts.Add(new Owner { Email = "szef@pizza.com", FirstName = "A", LastName = "B", PasswordHash = "hash", TaxId = "123" });
            await db.SaveChangesAsync();

            var googleMock = new Mock<IGoogleAuthService>();
            googleMock.Setup(g => g.ValidateAsync(It.IsAny<string>()))
                      .ReturnsAsync(new GoogleJsonWebSignature.Payload { Email = "szef@pizza.com" });

            var controller = new AuthController(db, GetConfig(), googleMock.Object);
            var result = await controller.GoogleLogin(new GoogleLoginDto { IdToken = "valid_token" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<LoginResponseDto>(okResult.Value);
            Assert.True(responseDto.IsOwner);
        }

        [Fact]
        public async Task GoogleLogin_InvalidToken_ReturnsBadRequest()
        {
            var googleMock = new Mock<IGoogleAuthService>();
            googleMock.Setup(g => g.ValidateAsync(It.IsAny<string>()))
                      .ThrowsAsync(new InvalidJwtException("Zły token"));

            var controller = new AuthController(GetDbContext(), GetConfig(), googleMock.Object);
            var result = await controller.GoogleLogin(new GoogleLoginDto { IdToken = "bad_token" });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Nieważny", badRequest.Value.ToString());
        }
    }
}
