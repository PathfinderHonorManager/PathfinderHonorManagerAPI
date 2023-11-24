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
