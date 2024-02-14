using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Tests.Helpers;
using PathfinderHonorManager.Validators;

namespace PathfinderHonorManager.Tests
{
    public class PathfinderAchievementValidatorTests
    {
        private DbContextOptions<PathfinderContext> _dbContextOptions;
        private PathfinderContext _dbContext;

        [SetUp]
        public async Task SetUp()
        {
            _dbContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new PathfinderContext(_dbContextOptions);
            await DatabaseSeeder.SeedDatabase(_dbContextOptions);
        }

        [TestCase]
        public async Task Validate_ExistingAchievementForPathfinder_ShouldFail()
        {
            // Arrange
            var validator = new PathfinderAchievementValidator(_dbContext);
            var existingAchievement = _dbContext.PathfinderAchievements.First();

            var dto = new Dto.Incoming.PathfinderAchievementDto
            {
                PathfinderID = existingAchievement.PathfinderID,
                AchievementID = existingAchievement.AchievementID
            };

            // Act
            var result = await validator
                .TestValidateAsync(dto, options =>
                {
                    options.IncludeAllRuleSets();
                });


            // Assert
            result.ShouldHaveValidationErrorFor(a => a.AchievementID)
                .WithSeverity(Severity.Error);
        }

        [TestCase]
        public async Task Validate_InvalidAchievementForPathfinder_ShouldFail()
        {
            // Arrange
            var validator = new PathfinderAchievementValidator(_dbContext);
            var existingAchievement = _dbContext.PathfinderAchievements.First();

            var dto = new Dto.Incoming.PathfinderAchievementDto
            {
                PathfinderID = existingAchievement.PathfinderID,
                AchievementID = Guid.NewGuid()
            };

            // Act
            var result = await validator
                .TestValidateAsync(dto, options =>
                {
                    options.IncludeAllRuleSets();
                });


            // Assert
            result.ShouldHaveValidationErrorFor(a => a.AchievementID)
                .WithSeverity(Severity.Error);
        }
        [TestCase]
        public async Task Validate_InvalidPathfinder_ShouldFail()
        {
            // Arrange
            var validator = new PathfinderAchievementValidator(_dbContext);
            var existingAchievement = _dbContext.PathfinderAchievements.First();

            var dto = new Dto.Incoming.PathfinderAchievementDto
            {
                PathfinderID = Guid.NewGuid(),
                AchievementID = existingAchievement.AchievementID
            };

            // Act
            var result = await validator
                .TestValidateAsync(dto, options =>
                {
                    options.IncludeAllRuleSets();
                });


            // Assert
            result.ShouldHaveValidationErrorFor(a => a.PathfinderID)
                .WithSeverity(Severity.Error);
        }

        [TestCase]
        public async Task Validate_AchievementGradeDoesNotMatchPathfinder_ShouldFail()
        {
            // Arrange
            var validator = new PathfinderAchievementValidator(_dbContext);
            var pathfinderAchievement = _dbContext.PathfinderAchievements.First();
            var achievement = await _dbContext.Achievements.FindAsync(pathfinderAchievement.AchievementID);
            achievement.Grade++;
            await _dbContext.SaveChangesAsync();

            var dto = new Dto.Incoming.PathfinderAchievementDto
            {
                PathfinderID = pathfinderAchievement.PathfinderID,
                AchievementID = pathfinderAchievement.AchievementID
            };

            // Act
            var result = await validator
                .TestValidateAsync(dto, options =>
                {
                    options.IncludeAllRuleSets();
                });

            // Assert
            result.ShouldHaveValidationErrorFor(a => a.AchievementID)
                .WithSeverity(Severity.Error);
        }

        [TearDown]
        public async Task TearDown()
        {
            await DatabaseCleaner.CleanDatabase(_dbContext);
            _dbContext.Dispose();
        }
    }
}