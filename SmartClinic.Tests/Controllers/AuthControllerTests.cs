using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartClinic.Controllers;
using SmartClinic.Data;
using SmartClinic.Models;
using Xunit;

namespace SmartClinic.Tests.Controllers
{
    public class AuthControllerTests
    {
        private DbContextOptions<ApplicationDbContext> GetDbOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
        }

        private AuthController CreateController(ApplicationDbContext context)
        {
            var config = new ConfigurationBuilder().Build();
            var emailService = new SmartClinic.Services.EmailService(config);
            var controller = new AuthController(context, emailService);
            var httpContext = new DefaultHttpContext();
            var session = new FakeSession();
            httpContext.Session = session;
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var tempData = new TempDataDictionary(httpContext, Moq.Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            return controller;
        }

        [Fact]
        public void Login_ValidCredentials_SetsSessionAndRedirects()
        {
            // Arrange
            var options = GetDbOptions();
            using (var context = new ApplicationDbContext(options))
            {
                context.Users.Add(new User { Id = 1, Name = "Admin User", Email = "admin@smartclinic.com", Code = "admin123", Password = BCrypt.Net.BCrypt.HashPassword("password"), Role = "Admin" });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateController(context);

                // Act
                var result = controller.Login("admin@smartclinic.com", "password") as RedirectToActionResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Index", result.ActionName);
                Assert.Equal("Dashboard", result.ControllerName);
                
                var session = controller.HttpContext.Session;
                Assert.Equal("Admin", session.GetString("Role"));
                Assert.Equal("Admin User", session.GetString("UserName"));
                Assert.Equal(1, session.GetInt32("UserId"));
            }
        }

        [Fact]
        public void Login_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var options = GetDbOptions();
            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateController(context);

                // Act
                var result = controller.Login("wrong@email.com", "credentials") as ViewResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Invalid email or password. Please try again.", controller.TempData["Error"]);
            }
        }
    }

    public class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;
        public string Id => "fake-session-id";
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }
}
