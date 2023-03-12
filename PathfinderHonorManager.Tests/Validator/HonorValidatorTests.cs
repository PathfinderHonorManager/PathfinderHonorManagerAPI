using System;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using NUnit.Framework;
using PathfinderHonorManager.Validators;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Tests
{
    [TestFixture]
    public class HonorValidatorTests
    {
        private HonorValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new HonorValidator();
        }

        [Test]
        public async Task Validate_HonorDto_WithValidData_ShouldPass()
        {
            // Arrange
            var honorDto = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 1,
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            // Act
            var result = await _validator.TestValidateAsync(honorDto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public async Task Validate_HonorDto_WithMissingName_ShouldFail()
        {
            // Arrange
            var honorDto = new Incoming.HonorDto
            {
                Level = 1,
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            // Act
            var result = await _validator.TestValidateAsync(honorDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Test]
        public async Task Validate_HonorDto_WithMissingLevel_ShouldFail()
        {
            // Arrange
            var honorDto = new Incoming.HonorDto
            {
                Name = "Test Honor",
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            // Act
            var result = await _validator.TestValidateAsync(honorDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Level);
        }

        [Test]
        public async Task Validate_HonorDto_WithInvalidPatchFilename_ShouldFail()
        {
            // Arrange
            var honorDto = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 1,
                PatchFilename = "invalid",
                WikiPath = new Uri("https://example.com")
            };

            // Act
            var result = await _validator.TestValidateAsync(honorDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PatchFilename);
        }

        [Test]
        public async Task Validate_HonorDto_WithInvalidWikiPath_ShouldFail()
        {
            // Arrange
            var honorDto = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 1,
                PatchFilename = "test.png",
                WikiPath = new Uri("invalid", UriKind.RelativeOrAbsolute)
            };

            // Act
            var result = await _validator.TestValidateAsync(honorDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.WikiPath);
        }

    }
}
