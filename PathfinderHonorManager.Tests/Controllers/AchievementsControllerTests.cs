using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PathfinderHonorManager.Controllers;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Service.Interfaces;
using PathfinderHonorManager.Tests.Helpers;
using PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Tests.Controllers
{
    public class AchievementsControllerTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        private AchievementsController _controller;
        private Mock<IAchievementService> _achievementServiceMock;
        private PathfinderContext _dbContext;

        [SetUp]
        public async Task Setup()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            
            _achievementServiceMock = new Mock<IAchievementService>();
            _controller = new AchievementsController(_achievementServiceMock.Object);
        }

        [Test]
        public async Task GetAchievements_WithValidData_ReturnsOkResult()
        {
            var expectedAchievements = new List<AchievementDto>
            {
                new AchievementDto
                {
                    AchievementID = Guid.NewGuid(),
                    Level = 1,
                    LevelName = "Basic",
                    AchievementSequenceOrder = 1,
                    Grade = 5,
                    ClassName = "Friend",
                    Description = "Test Achievement",
                    CategoryName = "Test Category",
                    CategorySequenceOrder = 1
                }
            };

            _achievementServiceMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAchievements);

            var result = await _controller.GetAchievements(new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedAchievements));
        }

        [Test]
        public async Task GetAchievements_WithNoAchievements_ReturnsNotFound()
        {
            _achievementServiceMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AchievementDto>());

            var result = await _controller.GetAchievements(new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetAchievementById_WithValidId_ReturnsOkResult()
        {
            var achievementId = Guid.NewGuid();
            var expectedAchievement = new AchievementDto
            {
                AchievementID = achievementId,
                Level = 1,
                LevelName = "Basic",
                AchievementSequenceOrder = 1,
                Grade = 5,
                ClassName = "Friend",
                Description = "Test Achievement",
                CategoryName = "Test Category",
                CategorySequenceOrder = 1
            };

            _achievementServiceMock
                .Setup(x => x.GetByIdAsync(achievementId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAchievement);

            var result = await _controller.GetAchievementById(achievementId, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedAchievement));
        }

        [Test]
        public async Task GetAchievementById_WithInvalidId_ReturnsNotFound()
        {
            var achievementId = Guid.NewGuid();

            _achievementServiceMock
                .Setup(x => x.GetByIdAsync(achievementId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AchievementDto)null);

            var result = await _controller.GetAchievementById(achievementId, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [TearDown]
        public async Task TearDown()
        {
            await DatabaseCleaner.CleanDatabase(_dbContext);
            await _dbContext.DisposeAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _dbContext.DisposeAsync();
        }
    }
} 