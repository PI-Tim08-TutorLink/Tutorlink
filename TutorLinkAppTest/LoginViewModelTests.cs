using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;

namespace TutorLinkAppTest
{
    public class LoginViewModelTests
    {
        private static List<ValidationResult> ValidateModel(LoginViewModel model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void LoginViewModel_ValidData_PassesValidation()
        {
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "password123",
                RememberMe = false
            };

            var results = ValidateModel(model);

            Assert.Empty(results);
        }

        [Fact]
        public void LoginViewModel_EmptyEmail_FailsValidation()
        {
            var model = new LoginViewModel
            {
                Email = "",
                Password = "password123"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void LoginViewModel_InvalidEmail_FailsValidation()
        {
            var model = new LoginViewModel
            {
                Email = "invalid-email",
                Password = "password123"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void LoginViewModel_EmptyPassword_FailsValidation()
        {
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = ""
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void LoginViewModel_NullEmail_FailsValidation()
        {
            var model = new LoginViewModel
            {
                Email = null!,
                Password = "password123"
            };

            var results = ValidateModel(model);

            Assert.NotEmpty(results);
        }

        [Fact]
        public void LoginViewModel_RememberMe_DefaultIsFalse()
        {
            var model = new LoginViewModel();

            Assert.False(model.RememberMe);
        }

        [Fact]
        public void LoginViewModel_RememberMe_CanBeSetToTrue()
        {
            var model = new LoginViewModel { RememberMe = true };

            Assert.True(model.RememberMe);
        }

        [Fact]
        public void LoginViewModel_ValidEmailFormats_PassValidation()
        {
            var validEmails = new[]
            {
                "test@example.com",
                "user.name@example.com",
                "user+tag@example.co.uk",
                "123@test.com"
            };

            foreach (var email in validEmails)
            {
                var model = new LoginViewModel
                {
                    Email = email,
                    Password = "password"
                };
                var results = ValidateModel(model);
                Assert.Empty(results);
            }
        }
    }
}
