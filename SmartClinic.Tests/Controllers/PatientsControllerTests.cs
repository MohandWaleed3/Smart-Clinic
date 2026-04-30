using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartClinic.Controllers;
using SmartClinic.Data;
using SmartClinic.Models;
using Xunit;

namespace SmartClinic.Tests.Controllers
{
    public class PatientsControllerTests
    {
        private DbContextOptions<ApplicationDbContext> GetDbOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private PatientsController CreateController(ApplicationDbContext context, string role = "Admin")
        {
            var controller = new PatientsController(context);
            var httpContext = new DefaultHttpContext();
            var session = new FakeSession();
            session.SetString("Role", role);
            httpContext.Session = session;
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public async Task Index_ReturnsAllPatients_WhenNoFilterApplied()
        {
            // Arrange
            var options = GetDbOptions();
            using (var context = new ApplicationDbContext(options))
            {
                context.Patients.Add(new Patient { Id = 1, Name = "Ahmed", Code = "P1", DateOfBirth = new DateTime(1990, 1, 1), Phone = "12345" });
                context.Patients.Add(new Patient { Id = 2, Name = "Sara", Code = "P2", DateOfBirth = new DateTime(1985, 5, 5), Phone = "12345" });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateController(context);

                // Act
                var result = await controller.Index(null, null) as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = result.Model as List<Patient>;
                Assert.NotNull(model);
                Assert.Equal(2, model.Count);
            }
        }

        [Fact]
        public async Task Index_FiltersByNameOrCode()
        {
            // Arrange
            var options = GetDbOptions();
            using (var context = new ApplicationDbContext(options))
            {
                context.Patients.Add(new Patient { Id = 1, Name = "Ahmed", Code = "P1", DateOfBirth = new DateTime(1990, 1, 1), Phone = "12345" });
                context.Patients.Add(new Patient { Id = 2, Name = "Sara", Code = "P2", DateOfBirth = new DateTime(1985, 5, 5), Phone = "12345" });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateController(context);

                // Act
                var result = await controller.Index("Sara", null) as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = result.Model as List<Patient>;
                Assert.NotNull(model);
                Assert.Single(model);
                Assert.Equal("Sara", model.First().Name);
            }
        }

        [Fact]
        public async Task Index_FiltersByDateOfBirth()
        {
            // Arrange
            var options = GetDbOptions();
            using (var context = new ApplicationDbContext(options))
            {
                context.Patients.Add(new Patient { Id = 1, Name = "Ahmed", Code = "P1", DateOfBirth = new DateTime(1990, 1, 1), Phone = "12345" });
                context.Patients.Add(new Patient { Id = 2, Name = "Sara", Code = "P2", DateOfBirth = new DateTime(1985, 5, 5), Phone = "12345" });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateController(context);

                // Act
                var result = await controller.Index(null, "1990-01-01") as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = result.Model as List<Patient>;
                Assert.NotNull(model);
                Assert.Single(model);
                Assert.Equal("Ahmed", model.First().Name);
            }
        }
    }
}
