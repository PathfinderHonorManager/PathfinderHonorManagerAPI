using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Moq;
using PathfinderHonorManager.DataAccess;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Tests.Helpers;
using PathfinderHonorManager.Dto.Outgoing;
using FluentAssertions;

namespace PathfinderHonorManager.Tests.Service
{
    public class PathfinderAchievementServiceTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        private List<Achievement> _achievements;
        private List<Pathfinder> _pathfinders;
        private List<PathfinderAchievement> _pathfinderAchievements;
        private PathfinderContext _dbContext;
        private PathfinderAchievementService _pathfinderAchievementService;

        [SetUp]
        public async Task Setup()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();

            var logger = NullLogger<PathfinderAchievementService>.Instance;
            var validator = new Mock<IValidator<Incoming.PathfinderAchievementDto>>();

            _pathfinderAchievementService = new PathfinderAchievementService(_dbContext, mapper, logger, validator.Object);
            _achievements = await _dbContext.Achievements.ToListAsync();
            _pathfinders = await _dbContext.Pathfinders.ToListAsync();
            _pathfinderAchievements = await _dbContext.PathfinderAchievements.ToListAsync();
        }

        [TestCase]
        public async Task GetAllAsync_ReturnsAllAchievements()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedCount = _pathfinderAchievements.Count;

            // Act
            var result = await _pathfinderAchievementService.GetAllAsync(cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ICollection<PathfinderAchievementDto>>());
            Assert.That(result.Count, Is.EqualTo(expectedCount));
        }

        [TestCase]
        public async Task GetByIdAsync_ValidId_ReturnsAchievement()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedPathfinderAchievement = _pathfinderAchievements.First();
            var expectedAchievementId = expectedPathfinderAchievement.AchievementID;
            var expectedPathfinderId = expectedPathfinderAchievement.PathfinderID;

            // Act
            var result = await _pathfinderAchievementService.GetByIdAsync(expectedPathfinderId, expectedAchievementId, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AchievementID, Is.EqualTo(expectedAchievementId));
            Assert.That(result.PathfinderID, Is.EqualTo(expectedPathfinderId));
            Assert.That(result, Is.InstanceOf<PathfinderAchievementDto>());
        }

        [TestCase]
        public async Task AddAsync_AddsNewPathfinderAchievementAndReturnsDto()
        {
            // Arrange
            var newAchievementDto = new Incoming.PostPathfinderAchievementDto
            {
                AchievementID = _achievements.First().AchievementID
            };
            var pathfinderId = _pathfinders.Last().PathfinderID;
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.AddAsync(pathfinderId, newAchievementDto, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PathfinderID, Is.EqualTo(pathfinderId));
            Assert.That(result.AchievementID, Is.EqualTo(newAchievementDto.AchievementID));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task UpdateAsync_UpdatesPathfinderAchievementAndReturnsUpdatedDto(bool isAchieved)
        {
            // Arrange
            var pathfinderAchievement = _pathfinderAchievements.First();
            var updateDto = new Incoming.PutPathfinderAchievementDto
            {
                IsAchieved = isAchieved
            };
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.UpdateAsync(pathfinderAchievement.PathfinderID, pathfinderAchievement.AchievementID, updateDto, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsAchieved, Is.EqualTo(isAchieved));
        }

        [Test]
        public async Task AddAchievementsForGradeAsync_AddsAchievementsForPathfinderGrade_ReturnsAchievements()
        {
            // Arrange
            var pathfinderId = _pathfinders.First(p => p.Grade == 10).PathfinderID; // Assuming grade 10 has specific achievements
            var expectedAchievements = _achievements.Where(a => a.Grade == 10).ToList();
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.AddAchievementsForGradeAsync(pathfinderId, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(expectedAchievements.Count));
            foreach (var achievement in expectedAchievements)
            {
                Assert.That(result.Any(ra => ra.AchievementID == achievement.AchievementID), Is.True);
            }
        }

        [Test]
        public async Task GetAllAchievementsForPathfinderAsync_ReturnsAllAchievementsForPathfinder()
        {
            // Arrange
            var pathfinderId = _pathfinders.First().PathfinderID; 
            var expectedAchievements = _pathfinderAchievements
                .Where(pa => pa.PathfinderID == pathfinderId)
                .Select(pa => pa.AchievementID)
                .ToList();
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.GetAllAchievementsForPathfinderAsync(pathfinderId, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<List<PathfinderAchievementDto>>());
            Assert.That(result.Count, Is.EqualTo(expectedAchievements.Count));
            foreach (var achievementId in expectedAchievements)
            {
                Assert.That(result.Any(ra => ra.AchievementID == achievementId), Is.True);
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            await DatabaseCleaner.CleanDatabase(_dbContext);
            _dbContext.Dispose();
        }
    }
}
