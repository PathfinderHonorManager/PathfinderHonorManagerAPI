using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public class ClubsControllerTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        private ClubsController _controller;
        private Mock<IClubService> _clubServiceMock;
        private Mock<ILogger<ClubsController>> _loggerMock;
        private PathfinderContext _dbContext;

        [SetUp]
        public async Task Setup()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            
            _clubServiceMock = new Mock<IClubService>();
            _loggerMock = new Mock<ILogger<ClubsController>>();
            _controller = new ClubsController(_clubServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetClubs_WithNoClubCode_ReturnsOkResult()
        {
            var expectedClubs = new List<Outgoing.ClubDto>
            {
                new Outgoing.ClubDto
                {
                    ClubID = Guid.NewGuid(),
                    ClubCode = "TEST1",
                    Name = "Test Club 1"
                },
                new Outgoing.ClubDto
                {
                    ClubID = Guid.NewGuid(),
                    ClubCode = "TEST2",
                    Name = "Test Club 2"
                }
            };

            _clubServiceMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedClubs);

            var result = await _controller.GetClubs(new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedClubs));
        }

        [Test]
        public async Task GetClubs_WithNoClubs_ReturnsNotFound()
        {
            _clubServiceMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<Outgoing.ClubDto>)null);

            var result = await _controller.GetClubs(new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetClubs_WithValidClubCode_ReturnsOkResult()
        {
            var clubCode = "TEST1";
            var expectedClub = new Outgoing.ClubDto
            {
                ClubID = Guid.NewGuid(),
                ClubCode = clubCode,
                Name = "Test Club 1"
            };

            _clubServiceMock
                .Setup(x => x.GetByCodeAsync(clubCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedClub);

            var result = await _controller.GetClubs(new CancellationToken(), clubCode);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedClub));
        }

        [Test]
        public async Task GetClubs_WithInvalidClubCode_ReturnsNotFound()
        {
            var clubCode = "INVALID";

            _clubServiceMock
                .Setup(x => x.GetByCodeAsync(clubCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.ClubDto)null);

            var result = await _controller.GetClubs(new CancellationToken(), clubCode);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ReturnsOkResult()
        {
            var clubId = Guid.NewGuid();
            var expectedClub = new Outgoing.ClubDto
            {
                ClubID = clubId,
                ClubCode = "TEST1",
                Name = "Test Club 1"
            };

            _clubServiceMock
                .Setup(x => x.GetByIdAsync(clubId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedClub);

            var result = await _controller.GetByIdAsync(clubId, new CancellationToken());

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedClub));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
        {
            var clubId = Guid.NewGuid();

            _clubServiceMock
                .Setup(x => x.GetByIdAsync(clubId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.ClubDto)null);

            var result = await _controller.GetByIdAsync(clubId, new CancellationToken());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task CreateAsync_WithValidData_ReturnsCreatedResult()
        {
            var newClub = new Incoming.ClubDto
            {
                ClubCode = "TEST1",
                Name = "Test Club 1"
            };

            var createdClub = new Outgoing.ClubDto
            {
                ClubID = Guid.NewGuid(),
                ClubCode = "TEST1",
                Name = "Test Club 1"
            };

            _clubServiceMock
                .Setup(x => x.CreateAsync(newClub, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClub);

            var result = await _controller.CreateAsync(newClub, new CancellationToken());

            Assert.That(result, Is.InstanceOf<CreatedAtRouteResult>());
            var createdResult = result as CreatedAtRouteResult;
            Assert.That(createdResult.Value, Is.EqualTo(createdClub));
            Assert.That(createdResult.RouteName, Is.EqualTo("GetClubById"));
            Assert.That(createdResult.RouteValues["id"], Is.EqualTo(createdClub.ClubID));
        }

        [Test]
        public async Task CreateAsync_WithValidationError_ReturnsValidationProblem()
        {
            var newClub = new Incoming.ClubDto
            {
                ClubCode = "TEST1",
                Name = "Test Club 1"
            };

            _clubServiceMock
                .Setup(x => x.CreateAsync(newClub, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation failed"));

            var result = await _controller.CreateAsync(newClub, new CancellationToken());

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
        }

        [Test]
        public async Task UpdateAsync_WithValidData_ReturnsOkResult()
        {
            var clubId = Guid.NewGuid();
            var updatedClub = new Incoming.ClubDto
            {
                ClubCode = "TEST1",
                Name = "Updated Test Club 1"
            };

            var expectedClub = new Outgoing.ClubDto
            {
                ClubID = clubId,
                ClubCode = "TEST1",
                Name = "Updated Test Club 1"
            };

            _clubServiceMock
                .Setup(x => x.UpdateAsync(clubId, updatedClub, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedClub);

            var result = await _controller.UpdateAsync(clubId, updatedClub, new CancellationToken());

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedClub));
        }

        [Test]
        public async Task UpdateAsync_WithInvalidId_ReturnsNotFound()
        {
            var clubId = Guid.NewGuid();
            var updatedClub = new Incoming.ClubDto
            {
                ClubCode = "TEST1",
                Name = "Updated Test Club 1"
            };

            _clubServiceMock
                .Setup(x => x.UpdateAsync(clubId, updatedClub, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.ClubDto)null);

            var result = await _controller.UpdateAsync(clubId, updatedClub, new CancellationToken());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateAsync_WithValidationError_ReturnsValidationProblem()
        {
            var clubId = Guid.NewGuid();
            var updatedClub = new Incoming.ClubDto
            {
                ClubCode = "TEST1",
                Name = "Updated Test Club 1"
            };

            _clubServiceMock
                .Setup(x => x.UpdateAsync(clubId, updatedClub, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation failed"));

            var result = await _controller.UpdateAsync(clubId, updatedClub, new CancellationToken());

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
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