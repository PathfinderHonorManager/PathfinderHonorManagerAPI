using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Validators;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Tests
{
    [TestFixture]
    public class HonorValidatorTests
    {
        private DbContextOptions<PathfinderContext> _dbContextOptions;
        private HonorValidator _validator;

        [SetUp]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new PathfinderContext(_dbContextOptions))
            {
                // Seed the database with some data
                context.Honors.Add(new Honor
                {
                    HonorID = Guid.NewGuid(),
                    Name = "Honor 1",
                    Level = 1,
                    PatchFilename = "patch1.png",
                    WikiPath = new Uri("https://example.com/honor1")
                });

                context.Honors.Add(new Honor
                {
                    HonorID = Guid.NewGuid(),
                    Name = "Honor 2",
                    Level = 2,
                    PatchFilename = "patch2.png",
                    WikiPath = new Uri("https://example.com/honor2")
                });

                context.SaveChanges();
            }

            _validator = new HonorValidator(new PathfinderContext(_dbContextOptions));
        }


        [TestCase]
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
            var result = await _validator
                .TestValidateAsync(honorDto);

            // Assert
            result
                .ShouldNotHaveAnyValidationErrors();
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task Validate_HonorDto_WithMissingName_ShouldFail(string name)
        {
            // Arrange
            var honorDto = new Incoming.HonorDto
            {
                Name = name,
                Level = 1,
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            // Act
            var result = await _validator
                .TestValidateAsync(honorDto);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.Name)
                .WithSeverity(Severity.Error);
        }

        [TestCase(null)]
        public async Task Validate_HonorDto_WithMissingLevel_ShouldFail(int level)
        {
            // Arrange
            var honorDto = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = level,
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            // Act
            var result = await _validator.TestValidateAsync(honorDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Level);
        }

        [TestCase("invalid")]
        public async Task Validate_HonorDto_WithInvalidPatchFilename_ShouldFail(string patchFilename)
        {
            // Arrange
            var honorDto = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 1,
                PatchFilename = patchFilename,
                WikiPath = new Uri("https://example.com")
            };

            // Act
            var result = await _validator.TestValidateAsync(honorDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PatchFilename);
        }

        [TestCase("invalid")]
        public async Task Validate_HonorDto_WithInvalidWikiPath_ShouldFail(string wikiPath)
        {
            // Arrange
            var honorDto = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 1,
                PatchFilename = "test.png",
                WikiPath = new Uri(wikiPath, UriKind.RelativeOrAbsolute)
            };

            // Act
            var result = await _validator.TestValidateAsync(honorDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.WikiPath);
        }

        [TestCase]
        public async Task Validate_HonorDto_WithDuplicateName_ShouldFail()
        {
            // Arrange
            var honorDto = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 1,
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            using (var context = new PathfinderContext(_dbContextOptions))
            {
                // Add a duplicate honor to the database
                var existingHonor = new Honor
                {
                    Name = honorDto.Name,
                    Level = honorDto.Level,
                    PatchFilename = honorDto.PatchFilename,
                    WikiPath = honorDto.WikiPath
                };
                await context.Honors.AddAsync(existingHonor);
                await context.SaveChangesAsync();
            }

            // Act
            var result = await _validator
                                .TestValidateAsync(honorDto, options =>
                                {
                                    options.IncludeAllRuleSets();
                                });

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }
    }
}
