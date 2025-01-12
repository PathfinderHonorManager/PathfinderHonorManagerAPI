using System;
using System.Threading.Tasks;
using FluentValidation;
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
        public void SetUp()
        {
            var context = new PathfinderContext(ContextOptions);
            _pathfinderValidator = new PathfinderValidator(context);
            SeedDatabase(context);
        }

        // Email tests
        [TestCase("")]
        [TestCase(null)]
        [TestCase("nonemailstring")]
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

        // FirstName tests
        [TestCase("")]
        [TestCase(null)]
        public async Task Validate_FirstName_ValidationError(string firstName)
        {
            var randEmail = RandomString(10);
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

        public static void SeedDatabase(PathfinderContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Pathfinders.AddRange(
                new Pathfinder[]
                {

                    new Pathfinder
                        {
                            FirstName = "test1",
                            LastName = "pathfinder",
                            Email = "test1@validemail.com",
                            Created = System.DateTime.Now,
                            Updated = System.DateTime.Now
                        },
                    new Pathfinder
                        {
                            FirstName = "test2",
                            LastName = "pathfinder",
                            Email = "test2@validemail.com",
                            Created = System.DateTime.Now,
                            Updated = System.DateTime.Now
                        },
                    new Pathfinder
                        {
                            FirstName = "test3",
                            LastName = "pathfinder",
                            Email = "test3@validemail.com",
                            Created = System.DateTime.Now,
                            Updated = System.DateTime.Now
                        }
                }); ;

            context.SaveChangesAsync();
        }
        public static string RandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);

            return finalString;
        }
    }
}

