using System;
using System.Linq;
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

namespace PathfinderHonorManager.Tests
{
    [TestFixture]
    public class PathfinderAchievementValidatorTests
    {
        private PathfinderAchievementValidator _achievementValidator;
        protected DbContextOptions<PathfinderContext> ContextOptions { get; }

        public PathfinderAchievementValidatorTests()
        {
            ContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        [SetUp]
        public async Task SetUp()
        {
            var context = new PathfinderContext(ContextOptions);
            _achievementValidator = new PathfinderAchievementValidator(context);
            await SeedDatabase(context);
        }

        [Test]
        public async Task Validate_InvalidAchievementID_ValidationError()
        {
            var newPathfinderAchievement = new Incoming.PathfinderAchievementDto
            {
                AchievementID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid()
            };

            var validationResult = await _achievementValidator
                .TestValidateAsync(newPathfinderAchievement, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.AchievementID)
                .WithSeverity(Severity.Error);
        }

        [Test]
        public async Task Validate_InvalidPathfinderID_ValidationError()
        {
            var newPathfinderAchievement = new Incoming.PathfinderAchievementDto
            {
                AchievementID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid()
            };

            var validationResult = await _achievementValidator
                .TestValidateAsync(newPathfinderAchievement, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID)
                .WithSeverity(Severity.Error);
        }

        [Test]
        public async Task Validate_DuplicateAchievementAssignment_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var existingAchievement = context.PathfinderAchievements.First();

                var newPathfinderAchievement = new Incoming.PathfinderAchievementDto
                {
                    AchievementID = existingAchievement.AchievementID,
                    PathfinderID = existingAchievement.PathfinderID
                };

                var validationResult = await _achievementValidator
                    .TestValidateAsync(newPathfinderAchievement, options =>
                    {
                        options.IncludeRuleSets("post");
                    });

                validationResult.ShouldHaveValidationErrorFor(p => p.AchievementID)
                    .WithSeverity(Severity.Error);
            }
        }

        [Test]
        public async Task Validate_GradeMismatch_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinder = context.Pathfinders.First();
                var achievement = context.Achievements.First(a => a.Grade != pathfinder.Grade);

                var newPathfinderAchievement = new Incoming.PathfinderAchievementDto
                {
                    AchievementID = achievement.AchievementID,
                    PathfinderID = pathfinder.PathfinderID
                };

                var validationResult = await _achievementValidator
                    .TestValidateAsync(newPathfinderAchievement);

                validationResult.ShouldHaveValidationErrorFor(p => p.AchievementID)
                    .WithSeverity(Severity.Error);
            }
        }

        [Test]
        public async Task Validate_ValidAchievement_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinder = context.Pathfinders.First();
                var achievement = context.Achievements
                    .Where(a => a.Grade == pathfinder.Grade)
                    .FirstOrDefault(a => !context.PathfinderAchievements
                        .Any(pa => pa.PathfinderID == pathfinder.PathfinderID && 
                                  pa.AchievementID == a.AchievementID));

                if (achievement == null)
                {
                    // Create a new achievement if all existing ones are assigned
                    achievement = new Achievement 
                    { 
                        AchievementID = Guid.NewGuid(),
                        Grade = pathfinder.Grade ?? 0
                    };
                    context.Achievements.Add(achievement);
                    await context.SaveChangesAsync();
                }

                var newPathfinderAchievement = new Incoming.PathfinderAchievementDto
                {
                    AchievementID = achievement.AchievementID,
                    PathfinderID = pathfinder.PathfinderID
                };

                var validationResult = await _achievementValidator
                    .TestValidateAsync(newPathfinderAchievement, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveAnyValidationErrors();
            }
        }

        [Test]
        public async Task Validate_NullPathfinder_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var dto = new Incoming.PathfinderAchievementDto
                {
                    AchievementID = Guid.NewGuid(),
                    PathfinderID = Guid.NewGuid()
                };

                var result = await _achievementValidator.TestValidateAsync(dto);

                result.ShouldNotHaveValidationErrorFor(x => x.AchievementID);
            }
        }

        [Test]
        public async Task Validate_NullAchievement_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinder = context.Pathfinders.First();
                var dto = new Incoming.PathfinderAchievementDto
                {
                    AchievementID = Guid.NewGuid(),
                    PathfinderID = pathfinder.PathfinderID
                };

                var result = await _achievementValidator.TestValidateAsync(dto);

                result.ShouldNotHaveValidationErrorFor(x => x.AchievementID);
            }
        }

        [Test]
        public async Task Validate_DuplicateAchievement_ShouldHaveCorrectErrorMessage()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var existingAchievement = context.PathfinderAchievements.First();
                var dto = new Incoming.PathfinderAchievementDto
                {
                    AchievementID = existingAchievement.AchievementID,
                    PathfinderID = existingAchievement.PathfinderID
                };

                var result = await _achievementValidator.TestValidateAsync(dto, opt => opt.IncludeRuleSets("post"));

                result.ShouldHaveValidationErrorFor(x => x.AchievementID)
                    .WithErrorMessage($"Pathfinder {dto.PathfinderID} has already been assigned achievement {dto.AchievementID}");
            }
        }

        [Test]
        public async Task Validate_InvalidPathfinderID_ShouldHaveCorrectErrorMessage()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var invalidPathfinderId = Guid.NewGuid();
                var dto = new Incoming.PathfinderAchievementDto
                {
                    AchievementID = context.Achievements.First().AchievementID,
                    PathfinderID = invalidPathfinderId
                };

                var result = await _achievementValidator.TestValidateAsync(dto, opt => opt.IncludeRuleSets("post"));

                result.ShouldHaveValidationErrorFor(x => x.PathfinderID)
                    .WithErrorMessage($"Invalid Pathfinder ID {invalidPathfinderId} provided.");
            }
        }

        [Test]
        public async Task Validate_InvalidAchievementID_ShouldHaveCorrectErrorMessage()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var invalidAchievementId = Guid.NewGuid();
                var dto = new Incoming.PathfinderAchievementDto
                {
                    AchievementID = invalidAchievementId,
                    PathfinderID = context.Pathfinders.First().PathfinderID
                };

                var result = await _achievementValidator.TestValidateAsync(dto, opt => opt.IncludeRuleSets("post"));

                result.ShouldHaveValidationErrorFor(x => x.AchievementID)
                    .WithErrorMessage($"Invalid Achievement ID {invalidAchievementId} provided.");
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                await DatabaseCleaner.CleanDatabase(context);
            }
        }

        private async Task SeedDatabase(PathfinderContext context)
        {
            await DatabaseSeeder.SeedDatabase(ContextOptions);
        }
    }
}