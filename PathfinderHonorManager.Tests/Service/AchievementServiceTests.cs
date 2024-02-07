using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Moq;
using PathfinderHonorManager.DataAccess;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Service.Interfaces;
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
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ICollection<AchievementDto>>(result);
            Assert.AreEqual(expectedCount, result.Count);
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
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedId, result.AchievementID);
            Assert.AreEqual(expectedAchievement.Description, result.Description);
        }
        
        [TearDown]
        public async Task TearDown()
        {
            await DatabaseCleaner.CleanDatabase(_dbContext);
            _dbContext.Dispose();
        }
    }
}
