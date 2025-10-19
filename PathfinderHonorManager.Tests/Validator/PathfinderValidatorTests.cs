﻿using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Tests.Helpers;
using PathfinderHonorManager.Validators;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Tests.Validator
{

    [TestFixture]
    public class PathfinderValidatorTests
    {
        public PathfinderValidatorTests()
        {
            ContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        private PathfinderValidator _pathfinderValidator;

        protected DbContextOptions<PathfinderContext> ContextOptions { get; }

        [SetUp]
        public async Task SetUp()
        {
            var context = new PathfinderContext(ContextOptions);
            await DatabaseCleaner.CleanDatabase(context);
            _pathfinderValidator = new PathfinderValidator(context);
            await DatabaseSeeder.SeedDatabase(ContextOptions);
        }

        // Email tests - invalid format should fail
        [TestCase("nonemailstring")]
        [TestCase("invalid@")]
        [TestCase("@invalid.com")]
        public async Task Validate_InvalidEmail_ValidationError(string email)
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "test",
                LastName = "user",
                Email = email
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.Email)
                .WithSeverity(Severity.Error);

        }

        // Email tests - empty/null emails should be allowed
        [TestCase("")]
        [TestCase(null)]
        public async Task Validate_EmptyOrNullEmail_ShouldPass(string email)
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "test",
                LastName = "user",
                Email = email
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldNotHaveValidationErrorFor(p => p.Email);
        }

        // FirstName tests
        [TestCase("")]
        [TestCase(null)]
        public async Task Validate_FirstName_ValidationError(string firstName)
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = firstName,
                LastName = "user",
                Email = "test@email.com"
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder);

            validationResult.ShouldHaveValidationErrorFor(p => p.FirstName)
                .WithSeverity(Severity.Error);

        }

        // LastName tests
        [TestCase("")]
        [TestCase(null)]
        public async Task Validate_LastName_ValidationError(string lastName)
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "test",
                LastName = lastName,
                Email = "test@email.com"
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder);

            validationResult.ShouldHaveValidationErrorFor(p => p.LastName)
                .WithSeverity(Severity.Error);
        }

        [Test]
        public async Task Validate_DuplicateEmail_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var existingEmail = "test@example.com";
                var existingPathfinder = new Pathfinder
                {
                    PathfinderID = Guid.NewGuid(),
                    FirstName = "Existing",
                    LastName = "User",
                    Email = existingEmail,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                };
                await context.Pathfinders.AddAsync(existingPathfinder);
                await context.SaveChangesAsync();

                var newPathfinder = new Incoming.PathfinderDtoInternal
                {
                    FirstName = "New",
                    LastName = "User",
                    Email = existingEmail,
                    ClubID = Guid.NewGuid()
                };

                var validationResult = await _pathfinderValidator
                    .TestValidateAsync(newPathfinder, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.Email)
                    .WithErrorMessage($"Pathfinder email address ({existingEmail}) is taken.");
            }
        }

        [Test]
        public async Task Validate_UniqueEmail_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var existingEmail = "test@example.com";
                var newEmail = "new@example.com";
                var existingPathfinder = new Pathfinder
                {
                    PathfinderID = Guid.NewGuid(),
                    FirstName = "Existing",
                    LastName = "User",
                    Email = existingEmail,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                };
                await context.Pathfinders.AddAsync(existingPathfinder);
                await context.SaveChangesAsync();

                var newPathfinder = new Incoming.PathfinderDtoInternal
                {
                    FirstName = "New",
                    LastName = "User",
                    Email = newEmail,
                    ClubID = Guid.NewGuid()
                };

                var validationResult = await _pathfinderValidator
                    .TestValidateAsync(newPathfinder, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveValidationErrorFor(p => p.Email);
            }
        }

        [Test]
        public async Task Validate_EmptyClubId_ValidationError()
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                ClubID = Guid.Empty
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder, options => options.IncludeRuleSets("post"));

            validationResult.ShouldHaveValidationErrorFor(p => p.ClubID)
                .WithErrorMessage("User must be associated with a valid club before adding a Pathfinder");
        }

        [Test]
        public async Task Validate_ValidClubId_ShouldPass()
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                ClubID = Guid.NewGuid()
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder, options => options.IncludeRuleSets("post"));

            validationResult.ShouldNotHaveValidationErrorFor(p => p.ClubID);
        }

        [Test]
        public async Task Validate_GradeOutOfRange_ValidationError()
        {
            var testCases = new[] { 4, 13 }; // Test both lower and upper bounds
            foreach (var grade in testCases)
            {
                var newPathfinder = new Incoming.PathfinderDtoInternal
                {
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    Grade = grade,
                    ClubID = Guid.NewGuid()
                };

                var validationResult = await _pathfinderValidator
                    .TestValidateAsync(newPathfinder);

                validationResult.ShouldHaveValidationErrorFor(p => p.Grade);
            }
        }

        [Test]
        public async Task Validate_GradeInRange_ShouldPass()
        {
            var testCases = new[] { 5, 8, 12 }; // Test lower bound, middle, and upper bound
            foreach (var grade in testCases)
            {
                var newPathfinder = new Incoming.PathfinderDtoInternal
                {
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    Grade = grade,
                    ClubID = Guid.NewGuid()
                };

                var validationResult = await _pathfinderValidator
                    .TestValidateAsync(newPathfinder);

                validationResult.ShouldNotHaveValidationErrorFor(p => p.Grade);
            }
        }

        [Test]
        public async Task Validate_AllRulesApplied_ValidationError()
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "",
                LastName = "",
                Email = "invalid",
                Grade = 4,
                ClubID = Guid.Empty
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder, options => 
                {
                    options.IncludeRuleSets("post");
                    options.IncludeRulesNotInRuleSet();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.FirstName);
            validationResult.ShouldHaveValidationErrorFor(p => p.LastName);
            validationResult.ShouldHaveValidationErrorFor(p => p.Email);
            validationResult.ShouldHaveValidationErrorFor(p => p.Grade);
            validationResult.ShouldHaveValidationErrorFor(p => p.ClubID);
        }

        [Test]
        public async Task Validate_ValidEmail_ShouldPass()
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "Test",
                LastName = "User",
                Email = "valid@example.com",
                ClubID = Guid.NewGuid()
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder, options => options.IncludeRuleSets("post"));

            validationResult.ShouldNotHaveValidationErrorFor(p => p.Email);
        }

        [Test]
        public async Task Validate_InvalidClubId_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var newPathfinder = new Incoming.PathfinderDtoInternal
                {
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    ClubID = Guid.NewGuid()
                };

                var validationResult = await _pathfinderValidator
                    .TestValidateAsync(newPathfinder, options => options.IncludeRuleSets("update"));

                validationResult.ShouldHaveValidationErrorFor(p => p.ClubID)
                    .WithErrorMessage("New club ID must be valid if provided");
            }
        }

        [Test]
        public async Task Validate_ValidClubIdInUpdate_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var club = new Club
                {
                    ClubID = Guid.NewGuid(),
                    Name = "Test Club",
                    ClubCode = "TEST"
                };
                await context.Clubs.AddAsync(club);
                await context.SaveChangesAsync();

                var newPathfinder = new Incoming.PathfinderDtoInternal
                {
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    ClubID = club.ClubID
                };

                var validationResult = await _pathfinderValidator
                    .TestValidateAsync(newPathfinder, options => options.IncludeRuleSets("update"));

                validationResult.ShouldNotHaveValidationErrorFor(p => p.ClubID);
            }
        }

        [Test]
        public async Task Validate_NullClubId_ShouldPass()
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                ClubID = null
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder, options => options.IncludeRuleSets("update"));

            validationResult.ShouldNotHaveValidationErrorFor(p => p.ClubID);
        }

        [Test]
        public async Task Validate_EmptyGuidClubId_ShouldPass()
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                ClubID = Guid.Empty
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder, options => options.IncludeRuleSets("update"));

            validationResult.ShouldNotHaveValidationErrorFor(p => p.ClubID);
        }

        [TearDown]
        public async Task TearDown()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                await DatabaseCleaner.CleanDatabase(context);
            }
        }

    }
}

