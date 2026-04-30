using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartClinic.Models;
using Xunit;

namespace SmartClinic.Tests.Models
{
    public class UserValidationTests
    {
        [Fact]
        public void User_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var user = new User
            {
                Name = "Test User",
                Code = "TEST01",
                Role = "Admin",
                Password = "validpassword"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(user, null, null);
            bool isValid = Validator.TryValidateObject(user, ctx, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void User_WithShortPassword_ShouldFailValidation()
        {
            // Arrange
            var user = new User
            {
                Name = "Test User",
                Code = "TEST01",
                Role = "Admin",
                Password = "short" // Less than 6 characters
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(user, null, null);
            bool isValid = Validator.TryValidateObject(user, ctx, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Password"));
        }

        [Fact]
        public void User_WithoutRequiredFields_ShouldFailValidation()
        {
            // Arrange
            var user = new User(); // All required fields empty

            // Act
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(user, null, null);
            bool isValid = Validator.TryValidateObject(user, ctx, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Equal(4, validationResults.Count); // Name, Code, Role, Password are required
        }
    }
}
