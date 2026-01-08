using Xunit;
using TutorLinkApp.Services.Implementations;
using System;
using System.Linq;

namespace TutorLinkApp.Tests.Services
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher;

        public PasswordHasherTests()
        {
            _hasher = new PasswordHasher();
        }

        [Fact]
        public void GenerateSalt_ReturnsNonEmptyString()
        {
            // Act
            var salt = _hasher.GenerateSalt();

            // Assert
            Assert.NotNull(salt);
            Assert.NotEmpty(salt);
        }

        [Fact]
        public void GenerateSalt_ReturnsBase64String()
        {
            // Act
            var salt = _hasher.GenerateSalt();

            // Assert
            Assert.NotNull(salt);
            var bytes = Convert.FromBase64String(salt);
            Assert.NotNull(bytes);
            Assert.Equal(32, bytes.Length);
        }

        [Fact]
        public void GenerateSalt_ReturnsUniqueValues()
        {
            // Act
            var salt1 = _hasher.GenerateSalt();
            var salt2 = _hasher.GenerateSalt();
            var salt3 = _hasher.GenerateSalt();

            // Assert
            Assert.NotEqual(salt1, salt2);
            Assert.NotEqual(salt2, salt3);
            Assert.NotEqual(salt1, salt3);
        }

        [Fact]
        public void GenerateSalt_Returns32ByteSalt()
        {
            // Act
            var salt = _hasher.GenerateSalt();
            var bytes = Convert.FromBase64String(salt);

            // Assert
            Assert.Equal(32, bytes.Length);
        }

        [Fact]
        public void GenerateSalt_GeneratesRandomSalts()
        {
            // Act
            var salts = Enumerable.Range(0, 10)
                .Select(_ => _hasher.GenerateSalt())
                .ToList();

            // Assert
            var uniqueSalts = salts.Distinct().Count();
            Assert.Equal(10, uniqueSalts);
        }

        // ========== HASH TESTS ==========

        [Fact]
        public void Hash_WithValidPasswordAndSalt_ReturnsHash()
        {
            // Arrange
            var password = "MySecurePassword123!";
            var salt = _hasher.GenerateSalt();

            // Act
            var hash = _hasher.Hash(password, salt);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Hash_ReturnsBase64String()
        {
            // Arrange
            var password = "TestPassword";
            var salt = _hasher.GenerateSalt();

            // Act
            var hash = _hasher.Hash(password, salt);

            // Assert
            var bytes = Convert.FromBase64String(hash);
            Assert.NotNull(bytes);
            Assert.Equal(32, bytes.Length);
        }

        [Fact]
        public void Hash_SamePasswordAndSalt_ProducesSameHash()
        {
            // Arrange
            var password = "MyPassword123";
            var salt = _hasher.GenerateSalt();

            // Act
            var hash1 = _hasher.Hash(password, salt);
            var hash2 = _hasher.Hash(password, salt);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void Hash_DifferentPasswords_ProduceDifferentHashes()
        {
            // Arrange
            var salt = _hasher.GenerateSalt();
            var password1 = "Password1";
            var password2 = "Password2";

            // Act
            var hash1 = _hasher.Hash(password1, salt);
            var hash2 = _hasher.Hash(password2, salt);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Hash_DifferentSalts_ProduceDifferentHashes()
        {
            // Arrange
            var password = "SamePassword";
            var salt1 = _hasher.GenerateSalt();
            var salt2 = _hasher.GenerateSalt();

            // Act
            var hash1 = _hasher.Hash(password, salt1);
            var hash2 = _hasher.Hash(password, salt2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Hash_EmptyPassword_ReturnsHash()
        {
            // Arrange
            var password = "";
            var salt = _hasher.GenerateSalt();

            // Act
            var hash = _hasher.Hash(password, salt);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Hash_SpecialCharactersInPassword_ReturnsHash()
        {
            // Arrange
            var password = "P@ssw0rd!#$%^&*()_+-=[]{}|;':\",./<>?";
            var salt = _hasher.GenerateSalt();

            // Act
            var hash = _hasher.Hash(password, salt);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Hash_UnicodeCharactersInPassword_ReturnsHash()
        {
            // Arrange
            var password = "Pässwörd123äöü你好";
            var salt = _hasher.GenerateSalt();

            // Act
            var hash = _hasher.Hash(password, salt);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Hash_LongPassword_ReturnsHash()
        {
            // Arrange
            var password = new string('a', 1000);
            var salt = _hasher.GenerateSalt();

            // Act
            var hash = _hasher.Hash(password, salt);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Hash_CaseSensitive()
        {
            // Arrange
            var salt = _hasher.GenerateSalt();
            var password1 = "Password";
            var password2 = "password";

            // Act
            var hash1 = _hasher.Hash(password1, salt);
            var hash2 = _hasher.Hash(password2, salt);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        // ========== VERIFY TESTS ==========

        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "MySecurePassword123!";
            var salt = _hasher.GenerateSalt();
            var hash = _hasher.Hash(password, salt);

            // Act
            var result = _hasher.Verify(password, hash, salt);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Verify_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var correctPassword = "CorrectPassword";
            var incorrectPassword = "WrongPassword";
            var salt = _hasher.GenerateSalt();
            var hash = _hasher.Hash(correctPassword, salt);

            // Act
            var result = _hasher.Verify(incorrectPassword, hash, salt);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Verify_WrongSalt_ReturnsFalse()
        {
            // Arrange
            var password = "MyPassword";
            var salt1 = _hasher.GenerateSalt();
            var salt2 = _hasher.GenerateSalt();
            var hash = _hasher.Hash(password, salt1);

            // Act
            var result = _hasher.Verify(password, hash, salt2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Verify_EmptyPassword_WorksCorrectly()
        {
            // Arrange
            var password = "";
            var salt = _hasher.GenerateSalt();
            var hash = _hasher.Hash(password, salt);

            // Act
            var correctResult = _hasher.Verify("", hash, salt);
            var incorrectResult = _hasher.Verify("notEmpty", hash, salt);

            // Assert
            Assert.True(correctResult);
            Assert.False(incorrectResult);
        }

        [Fact]
        public void Verify_CaseSensitive()
        {
            // Arrange
            var password = "Password123";
            var salt = _hasher.GenerateSalt();
            var hash = _hasher.Hash(password, salt);

            // Act
            var correctResult = _hasher.Verify("Password123", hash, salt);
            var incorrectResult = _hasher.Verify("password123", hash, salt);

            // Assert
            Assert.True(correctResult);
            Assert.False(incorrectResult);
        }

        [Fact]
        public void Verify_SpecialCharacters_WorksCorrectly()
        {
            // Arrange
            var password = "P@ss!#$%^&*()";
            var salt = _hasher.GenerateSalt();
            var hash = _hasher.Hash(password, salt);

            // Act
            var result = _hasher.Verify(password, hash, salt);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Verify_UnicodeCharacters_WorksCorrectly()
        {
            // Arrange
            var password = "Pässwörd你好";
            var salt = _hasher.GenerateSalt();
            var hash = _hasher.Hash(password, salt);

            // Act
            var result = _hasher.Verify(password, hash, salt);

            // Assert
            Assert.True(result);
        }

        // ========== INTEGRATION TESTS ==========

        [Fact]
        public void FullWorkflow_GenerateSaltHashAndVerify_WorksCorrectly()
        {
            // Arrange
            var password = "UserPassword123!";

            // Act
            var salt = _hasher.GenerateSalt();
            var hash = _hasher.Hash(password, salt);

            var loginSuccessful = _hasher.Verify(password, hash, salt);

            var loginFailed = _hasher.Verify("WrongPassword", hash, salt);

            // Assert
            Assert.True(loginSuccessful);
            Assert.False(loginFailed);
        }

        [Fact]
        public void MultipleUsers_DifferentSalts_ProduceDifferentHashes()
        {
            // Arrange
            var password = "CommonPassword123";

            var salt1 = _hasher.GenerateSalt();
            var hash1 = _hasher.Hash(password, salt1);

            var salt2 = _hasher.GenerateSalt();
            var hash2 = _hasher.Hash(password, salt2);

            // Assert
            Assert.NotEqual(salt1, salt2);
            Assert.NotEqual(hash1, hash2);

            Assert.True(_hasher.Verify(password, hash1, salt1));
            Assert.True(_hasher.Verify(password, hash2, salt2));

            Assert.False(_hasher.Verify(password, hash1, salt2));
            Assert.False(_hasher.Verify(password, hash2, salt1));
        }

        [Fact]
        public void Hash_DeterministicOutput()
        {
            // Arrange
            var password = "TestPassword";
            var salt = _hasher.GenerateSalt();

            // Act
            var hashes = Enumerable.Range(0, 5)
                .Select(_ => _hasher.Hash(password, salt))
                .ToList();

            // Assert
            Assert.True(hashes.All(h => h == hashes[0]));
        }
    }
}