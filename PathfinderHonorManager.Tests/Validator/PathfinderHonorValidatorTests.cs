using System;
using FluentValidation;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Validators;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Tests.Validator
{
    [TestFixture]
    public class PathfinderHonorValidatorTests
    {
        public PathfinderHonorValidatorTests()
        {
            ContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        private PathfinderHonorValidator _pathfinderHonorValidator;

        protected DbContextOptions<PathfinderContext> ContextOptions { get; }

        [SetUp]
        public void SetUp()
        {
            var context = new PathfinderContext(ContextOptions);
            _pathfinderHonorValidator = new PathfinderHonorValidator(context);
            SeedDatabase(context);
        }

        [Test]
        public async Task Validate_InvalidHonorID_ValidationError()
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = 1
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                .WithSeverity(Severity.Error);
        }

        [Test]
        public async Task Validate_InvalidPathfinderID_ValidationError()
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = 1
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID)
                .WithSeverity(Severity.Error);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task Validate_InvalidStatusCode_ValidationError(int invalidStatusCode)
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = invalidStatusCode
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.StatusCode)
                .WithSeverity(Severity.Error);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task Validate_ValidStatusCode_ShouldPass(int statusCode)
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = statusCode
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor);

            validationResult.ShouldNotHaveValidationErrorFor(p => p.StatusCode);
        }

        [Test]
        public async Task Validate_DuplicateHonorAssignment_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                // Create and save test data
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options =>
                    {
                        options.IncludeRuleSets("post");
                    });

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithSeverity(Severity.Error);
            }
        }

        [Test]
        public async Task Validate_MultipleValidationRules_ValidationError()
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.Empty,
                PathfinderID = Guid.Empty,
                StatusCode = -1
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor, options =>
                {
                    options.IncludeRuleSets("post");
                    options.IncludeRulesNotInRuleSet();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.StatusCode);
            validationResult.ShouldHaveValidationErrorFor(p => p.HonorID);
            validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID);
        }

        [Test]
        public async Task Validate_StatusTransition_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                // Create initial honor with Planned status
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId,
                    StatusCode = 1, // Planned
                    Created = DateTime.UtcNow
                };
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                // Try to create duplicate with different status
                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 2 // Earned
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options =>
                    {
                        options.IncludeRuleSets("post");
                    });

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithSeverity(Severity.Error);
            }
        }

        [Test]
        public async Task Validate_NonexistentPathfinder_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var honorId = Guid.NewGuid();
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                await context.Honors.AddAsync(honor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = Guid.NewGuid(), // Non-existent pathfinder
                    StatusCode = 1,
                    Status = "Planned"
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options =>
                    {
                        options.IncludeRuleSets("post");
                    });

                validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID)
                    .WithSeverity(Severity.Error);
            }
        }

        public static void SeedDatabase(PathfinderContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var validPathfinderID = Guid.NewGuid();
            var validHonorID = Guid.NewGuid();

            context.Pathfinders.Add(
                new Pathfinder
                {
                    PathfinderID = validPathfinderID,
                    FirstName = "test1",
                    LastName = "pathfinder",
                    Email = "test1@validemail.com",
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                });

            context.Honors.Add(
                new Honor
                {
                    HonorID = validHonorID
                });

            context.SaveChangesAsync();
        }
    }
}
