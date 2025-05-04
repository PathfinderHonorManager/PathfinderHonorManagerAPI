using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PathfinderHonorManager.Controllers;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Service.Interfaces;
using PathfinderHonorManager.Tests.Helpers;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Tests.Controllers
{
    public class PathfinderAchievementsControllerTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        private PathfinderAchievementsController _controller;
        private Mock<IPathfinderAchievementService> _pathfinderAchievementServiceMock;
        private PathfinderContext _dbContext;

        [SetUp]
        public async Task Setup()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            
            _pathfinderAchievementServiceMock = new Mock<IPathfinderAchievementService>();
            _controller = new PathfinderAchievementsController(_pathfinderAchievementServiceMock.Object);
        }

        [Test]
        public async Task GetPathfinderAchievements_WithValidData_ReturnsOkResult()
        {
            var expectedAchievements = new List<Outgoing.PathfinderAchievementDto>
            {
                new Outgoing.PathfinderAchievementDto
                {
                    PathfinderAchievementID = Guid.NewGuid(),
                    PathfinderID = Guid.NewGuid(),
                    AchievementID = Guid.NewGuid(),
                    IsAchieved = true,
                    Level = 1,
                    Grade = 5,
                    Description = "Test Achievement"
                }
            };

            _pathfinderAchievementServiceMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAchievements);

            var result = await _controller.GetPathfinderAchievements(new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedAchievements));
        }

        [Test]
        public async Task GetPathfinderAchievements_WithNoAchievements_ReturnsNotFound()
        {
            _pathfinderAchievementServiceMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Outgoing.PathfinderAchievementDto>());

            var result = await _controller.GetPathfinderAchievements(new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetPathfinderAchievementById_WithValidIds_ReturnsOkResult()
        {
            var pathfinderId = Guid.NewGuid();
            var achievementId = Guid.NewGuid();
            var expectedAchievement = new Outgoing.PathfinderAchievementDto
            {
                PathfinderAchievementID = Guid.NewGuid(),
                PathfinderID = pathfinderId,
                AchievementID = achievementId,
                IsAchieved = true,
                Level = 1,
                Grade = 5,
                Description = "Test Achievement"
            };

            _pathfinderAchievementServiceMock
                .Setup(x => x.GetByIdAsync(pathfinderId, achievementId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAchievement);

            var result = await _controller.GetPathfinderAchievementById(pathfinderId, achievementId, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedAchievement));
        }

        [Test]
        public async Task GetPathfinderAchievementById_WithInvalidIds_ReturnsNotFound()
        {
            var pathfinderId = Guid.NewGuid();
            var achievementId = Guid.NewGuid();

            _pathfinderAchievementServiceMock
                .Setup(x => x.GetByIdAsync(pathfinderId, achievementId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.PathfinderAchievementDto)null);

            var result = await _controller.GetPathfinderAchievementById(pathfinderId, achievementId, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task PostAsync_WithValidData_ReturnsCreatedResult()
        {
            var pathfinderId = Guid.NewGuid();
            var achievementId = Guid.NewGuid();
            var newAchievement = new Incoming.PostPathfinderAchievementDto
            {
                AchievementID = achievementId
            };

            var createdAchievement = new Outgoing.PathfinderAchievementDto
            {
                PathfinderAchievementID = Guid.NewGuid(),
                PathfinderID = pathfinderId,
                AchievementID = achievementId,
                IsAchieved = false,
                Level = 1,
                Grade = 5,
                Description = "Test Achievement"
            };

            _pathfinderAchievementServiceMock
                .Setup(x => x.AddAsync(pathfinderId, newAchievement, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdAchievement);

            var result = await _controller.PostAsync(pathfinderId, newAchievement, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult.Value, Is.EqualTo(createdAchievement));
            Assert.That(createdResult.ActionName, Is.EqualTo(nameof(PathfinderAchievementsController.GetPathfinderAchievementById)));
            Assert.That(createdResult.RouteValues["pathfinderId"], Is.EqualTo(pathfinderId));
            Assert.That(createdResult.RouteValues["achievementId"], Is.EqualTo(achievementId));
        }

        [Test]
        public async Task PostAsync_WithValidationError_ReturnsValidationProblem()
        {
            var pathfinderId = Guid.NewGuid();
            var newAchievement = new Incoming.PostPathfinderAchievementDto
            {
                AchievementID = Guid.NewGuid()
            };

            _pathfinderAchievementServiceMock
                .Setup(x => x.AddAsync(pathfinderId, newAchievement, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation failed"));

            var result = await _controller.PostAsync(pathfinderId, newAchievement, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result.Result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
        }

        [Test]
        public async Task UpdatePathfinderAchievement_WithValidData_ReturnsOkResult()
        {
            var pathfinderId = Guid.NewGuid();
            var achievementId = Guid.NewGuid();
            var updatedAchievement = new Incoming.PutPathfinderAchievementDto
            {
                IsAchieved = true
            };

            var expectedAchievement = new Outgoing.PathfinderAchievementDto
            {
                PathfinderAchievementID = Guid.NewGuid(),
                PathfinderID = pathfinderId,
                AchievementID = achievementId,
                IsAchieved = true,
                Level = 1,
                Grade = 5,
                Description = "Test Achievement"
            };

            _pathfinderAchievementServiceMock
                .Setup(x => x.UpdateAsync(pathfinderId, achievementId, updatedAchievement, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAchievement);

            var result = await _controller.UpdatePathfinderAchievement(pathfinderId, achievementId, updatedAchievement, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedAchievement));
        }

        [Test]
        public async Task UpdatePathfinderAchievement_WithInvalidIds_ReturnsNotFound()
        {
            var pathfinderId = Guid.NewGuid();
            var achievementId = Guid.NewGuid();
            var updatedAchievement = new Incoming.PutPathfinderAchievementDto
            {
                IsAchieved = true
            };

            _pathfinderAchievementServiceMock
                .Setup(x => x.UpdateAsync(pathfinderId, achievementId, updatedAchievement, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.PathfinderAchievementDto)null);

            var result = await _controller.UpdatePathfinderAchievement(pathfinderId, achievementId, updatedAchievement, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetAllAchievementsForPathfinder_WithValidId_ReturnsOkResult()
        {
            var pathfinderId = Guid.NewGuid();
            var expectedAchievements = new List<Outgoing.PathfinderAchievementDto>
            {
                new Outgoing.PathfinderAchievementDto
                {
                    PathfinderAchievementID = Guid.NewGuid(),
                    PathfinderID = pathfinderId,
                    AchievementID = Guid.NewGuid(),
                    IsAchieved = true,
                    Level = 1,
                    Grade = 5,
                    Description = "Test Achievement 1"
                },
                new Outgoing.PathfinderAchievementDto
                {
                    PathfinderAchievementID = Guid.NewGuid(),
                    PathfinderID = pathfinderId,
                    AchievementID = Guid.NewGuid(),
                    IsAchieved = false,
                    Level = 2,
                    Grade = 5,
                    Description = "Test Achievement 2"
                }
            };

            _pathfinderAchievementServiceMock
                .Setup(x => x.GetAllAchievementsForPathfinderAsync(pathfinderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAchievements);

            var result = await _controller.GetAllAchievementsForPathfinder(pathfinderId, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedAchievements));
        }

        [Test]
        public async Task GetAllAchievementsForPathfinder_WithInvalidId_ReturnsNotFound()
        {
            var pathfinderId = Guid.NewGuid();

            _pathfinderAchievementServiceMock
                .Setup(x => x.GetAllAchievementsForPathfinderAsync(pathfinderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Outgoing.PathfinderAchievementDto>());

            var result = await _controller.GetAllAchievementsForPathfinder(pathfinderId, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task AddAchievementsForGrade_WithValidData_ReturnsMultiStatus()
        {
            var pathfinderId = Guid.NewGuid();
            var dto = new Incoming.PostPathfinderAchievementForGradeDto
            {
                PathfinderIds = new List<Guid> { pathfinderId }
            };

            var createdAchievements = new List<Outgoing.PathfinderAchievementDto>
            {
                new Outgoing.PathfinderAchievementDto
                {
                    PathfinderAchievementID = Guid.NewGuid(),
                    PathfinderID = pathfinderId,
                    AchievementID = Guid.NewGuid(),
                    IsAchieved = false,
                    Level = 1,
                    Grade = 5,
                    Description = "Test Achievement"
                }
            };

            _pathfinderAchievementServiceMock
                .Setup(x => x.AddAchievementsForPathfinderAsync(pathfinderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdAchievements);

            var result = await _controller.AddAchievementsForGrade(dto, new CancellationToken());

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var multiStatusResult = result as ObjectResult;
            Assert.That(multiStatusResult.StatusCode, Is.EqualTo(207));
        }

        [TearDown]
        public async Task TearDown()
        {
            await DatabaseCleaner.CleanDatabase(_dbContext);
            _dbContext.Dispose();
        }
    }
} 