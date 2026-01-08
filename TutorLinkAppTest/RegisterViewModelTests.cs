using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;

namespace TutorLinkAppTest
{
    public class RegisterViewModelTests
    {
        private List<ValidationResult> ValidateModel(RegisterViewModel model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void RegisterViewModel_ValidData_PassesValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Password = "password123",
                ConfirmPassword = "password123",
                Role = "Student"
            };

            var results = ValidateModel(model);

            Assert.Empty(results);
        }

        [Fact]
        public void RegisterViewModel_EmptyEmail_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Password = "password123",
                ConfirmPassword = "password123",
                Role = "Student"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterViewModel_InvalidEmail_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "invalid-email",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Password = "password123",
                ConfirmPassword = "password123",
                Role = "Student"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterViewModel_PasswordTooShort_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Password = "12345",
                ConfirmPassword = "12345",
                Role = "Student"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void RegisterViewModel_PasswordsDoNotMatch_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Password = "password123",
                ConfirmPassword = "different456",
                Role = "Student"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("ConfirmPassword"));
        }

        [Fact]
        public void RegisterViewModel_EmptyUsername_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = "",
                FirstName = "John",
                LastName = "Doe",
                Password = "password123",
                ConfirmPassword = "password123",
                Role = "Student"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
        }

        [Fact]
        public void RegisterViewModel_UsernameTooLong_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = new string('a', 51), // 51 characters, max is 50
                FirstName = "John",
                LastName = "Doe",
                Password = "password123",
                ConfirmPassword = "password123",
                Role = "Student"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
        }

        [Fact]
        public void RegisterViewModel_FirstNameTooLong_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = new string('a', 257), // 257 characters, max is 256
                LastName = "Doe",
                Password = "password123",
                ConfirmPassword = "password123",
                Role = "Student"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void RegisterViewModel_LastNameTooLong_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = new string('a', 257), // 257 characters, max is 256
                Password = "password123",
                ConfirmPassword = "password123",
                Role = "Student"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("LastName"));
        }

        [Fact]
        public void RegisterViewModel_EmptyRole_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Password = "password123",
                ConfirmPassword = "password123",
                Role = ""
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Role"));
        }

        [Fact]
        public void RegisterViewModel_SkillsOptional_PassesValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Password = "password123",
                ConfirmPassword = "password123",
                Role = "Student",
                Skills = null // Optional
            };

            var results = ValidateModel(model);

            Assert.Empty(results);
        }

        [Fact]
        public void RegisterViewModel_SkillsTooLong_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Password = "password123",
                ConfirmPassword = "password123",
                Role = "Tutor",
                Skills = new string('a', 257) // 257 characters, max is 256
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Skills"));
        }

        [Fact]
        public void RegisterViewModel_MultipleErrors_ReturnsAllErrors()
        {
            var model = new RegisterViewModel
            {
                Email = "invalid-email",
                Username = "",
                FirstName = "",
                LastName = "",
                Password = "123", // Too short
                ConfirmPassword = "456", // Doesn't match
                Role = ""
            };

            var results = ValidateModel(model);

            Assert.True(results.Count >= 5); // Multiple validation errors
        }

        [Fact]
        public void RegisterViewModel_MinimumValidPassword_PassesValidation()
        {
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Password = "123456", // Exactly 6 characters (minimum)
                ConfirmPassword = "123456",
                Role = "Student"
            };

            var results = ValidateModel(model);

            Assert.Empty(results);
        }
    }
}
