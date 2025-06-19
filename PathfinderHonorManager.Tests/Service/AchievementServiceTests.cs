using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Tests.Helpers;
using PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Tests.Service
{
    public class AchievementServiceTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        private List<Achievement> _achievements;
        private PathfinderContext _dbContext;
        private AchievementService _achievementService;

        [SetUp]
        public async Task Setup()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();

            var logger = NullLogger<AchievementService>.Instance;

            _achievementService = new AchievementService(_dbContext, mapper, logger);
            _achievements = await _dbContext.Achievements.ToListAsync();
        }

        [TestCase]
        public async Task GetAllAsync_ReturnsAllAchievements()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedCount = _achievements.Count;

            // Act
            var result = await _achievementService.GetAllAsync(cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ICollection<AchievementDto>>());
            Assert.That(result.Count, Is.EqualTo(expectedCount));
        }

        [TestCase]
        public async Task GetByIdAsync_ValidId_ReturnsAchievement()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedAchievement = _achievements.First();
            var expectedId = expectedAchievement.AchievementID;

            // Act
            var result = await _achievementService.GetByIdAsync(expectedId, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AchievementID, Is.EqualTo(expectedId));
            Assert.That(result.Description, Is.EqualTo(expectedAchievement.Description));
        }

        [TestCase]
        public async Task GetAllAsync_ReturnsAchievementsWithCategoryInfo()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _achievementService.GetAllAsync(cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ICollection<AchievementDto>>());
            Assert.That(result.Count, Is.GreaterThan(0));
            
            // Verify that category information is included
            foreach (var achievement in result)
            {
                Assert.That(achievement.CategoryName, Is.Not.Null, "CategoryName should not be null");
                Assert.That(achievement.CategorySequenceOrder, Is.GreaterThanOrEqualTo(0), "CategorySequenceOrder should be a valid number");
            }
        }

        [TestCase]
        public async Task GetByIdAsync_ReturnsAchievementWithCategoryInfo()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedAchievement = _achievements.First();
            var expectedId = expectedAchievement.AchievementID;

            // Act
            var result = await _achievementService.GetByIdAsync(expectedId, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AchievementID, Is.EqualTo(expectedId));
            Assert.That(result.CategoryName, Is.Not.Null, "CategoryName should not be null");
            Assert.That(result.CategorySequenceOrder, Is.GreaterThanOrEqualTo(0), "CategorySequenceOrder should be a valid number");
        }

        [TestCase]
        public async Task Debug_CategoryNavigationProperty()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var achievement = await _dbContext.Achievements
                .Include(a => a.Category)
                .FirstAsync(cancellationToken);

            // Act & Assert
            Assert.That(achievement.Category, Is.Not.Null, "Category navigation property should not be null");
            Assert.That(achievement.Category.CategoryName, Is.Not.Null, "Category.CategoryName should not be null");
            Assert.That(achievement.Category.CategorySequenceOrder, Is.GreaterThanOrEqualTo(0), "Category.CategorySequenceOrder should be a valid number");
        }

        [TearDown]
        public async Task TearDown()
        {
            await DatabaseCleaner.CleanDatabase(_dbContext);
            _dbContext.Dispose();
        }
    }
}
