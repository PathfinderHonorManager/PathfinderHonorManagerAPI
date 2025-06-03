using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
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
    public class PathfindersControllerTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        private PathfindersController _controller;
        private Mock<IPathfinderService> _pathfinderServiceMock;
        private Mock<ILogger<PathfindersController>> _loggerMock;
        private PathfinderContext _dbContext;
        private const string TestClubCode = "TEST";

        [SetUp]
        public async Task Setup()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            
            _pathfinderServiceMock = new Mock<IPathfinderService>();
            _loggerMock = new Mock<ILogger<PathfindersController>>();
            _controller = new PathfindersController(_pathfinderServiceMock.Object, _loggerMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("clubCode", TestClubCode)
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetAll_WithValidClubCode_ReturnsOkResult()
        {
            var expectedPathfinders = new List<Outgoing.PathfinderDependantDto>
            {
                new Outgoing.PathfinderDependantDto
                {
                    PathfinderID = Guid.NewGuid(),
                    FirstName = "Test",
                    LastName = "User",
                    Grade = 5,
                    IsActive = true
                }
            };

            _pathfinderServiceMock
                .Setup(x => x.GetAllAsync(TestClubCode, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPathfinders);

            var result = await _controller.GetAll(new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedPathfinders));
        }

        [Test]
        public async Task GetAll_WithNoPathfinders_ReturnsNotFound()
        {
            _pathfinderServiceMock
                .Setup(x => x.GetAllAsync(TestClubCode, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<Outgoing.PathfinderDependantDto>)null);

            var result = await _controller.GetAll(new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetById_WithValidIds_ReturnsOkResult()
        {
            var pathfinderId = Guid.NewGuid();
            var expectedPathfinder = new Outgoing.PathfinderDependantDto
            {
                PathfinderID = pathfinderId,
                FirstName = "Test",
                LastName = "User",
                Grade = 5,
                IsActive = true
            };

            _pathfinderServiceMock
                .Setup(x => x.GetByIdAsync(pathfinderId, TestClubCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPathfinder);

            var result = await _controller.GetByIdAsync(pathfinderId, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedPathfinder));
        }

        [Test]
        public async Task GetById_WithInvalidIds_ReturnsNotFound()
        {
            var pathfinderId = Guid.NewGuid();

            _pathfinderServiceMock
                .Setup(x => x.GetByIdAsync(pathfinderId, TestClubCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.PathfinderDependantDto)null);

            var result = await _controller.GetByIdAsync(pathfinderId, new CancellationToken());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Post_WithValidData_ReturnsCreatedAtRouteWithCorrectRouteValues()
        {
            var newPathfinder = new Incoming.PathfinderDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Grade = 5,
                IsActive = true
            };

            var createdPathfinderId = Guid.NewGuid();
            var createdPathfinder = new Outgoing.PathfinderDto
            {
                PathfinderID = createdPathfinderId,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Grade = 5,
                IsActive = true
            };

            _pathfinderServiceMock
                .Setup(x => x.AddAsync(newPathfinder, TestClubCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPathfinder);

            var result = await _controller.PostAsync(newPathfinder, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<CreatedAtRouteResult>());
            
            var createdResult = result as CreatedAtRouteResult;
            Assert.That(createdResult.Value, Is.EqualTo(createdPathfinder));
            Assert.That(createdResult.RouteName, Is.EqualTo("GetPathfinderById"));
            Assert.That(createdResult.RouteValues, Is.Not.Null);
            Assert.That(createdResult.RouteValues["id"], Is.EqualTo(createdPathfinderId));
        }

        [Test]
        public async Task Post_WithValidationException_Returns400BadRequest()
        {
            var newPathfinder = new Incoming.PathfinderDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "duplicate@example.com",
                Grade = 5,
                IsActive = true
            };

            var validationErrors = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Email", "Pathfinder email address (duplicate@example.com) is taken.")
            };
            var validationException = new ValidationException(validationErrors);

            _pathfinderServiceMock
                .Setup(x => x.AddAsync(newPathfinder, TestClubCode, It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var result = await _controller.PostAsync(newPathfinder, new CancellationToken());

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
            
            var validationProblem = badRequestResult.Value as ValidationProblemDetails;
            Assert.That(validationProblem.Errors, Contains.Key("Email"));
        }

        [Test]
        public async Task Post_WithDbError_ReturnsValidationProblem()
        {
            var newPathfinder = new Incoming.PathfinderDto
            {
                FirstName = "Test",
                LastName = "User",
                Grade = 5,
                IsActive = true
            };

            _pathfinderServiceMock
                .Setup(x => x.AddAsync(newPathfinder, TestClubCode, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Database error"));

            var result = await _controller.PostAsync(newPathfinder, new CancellationToken());

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
        }

        [Test]
        public async Task Put_WithValidData_ReturnsOkResult()
        {
            var pathfinderId = Guid.NewGuid();
            var updatePathfinder = new Incoming.PutPathfinderDto
            {
                Grade = 6,
                IsActive = true
            };

            var updatedPathfinder = new Outgoing.PathfinderDto
            {
                PathfinderID = pathfinderId,
                FirstName = "Test",
                LastName = "User",
                Grade = 6,
                IsActive = true
            };

            _pathfinderServiceMock
                .Setup(x => x.UpdateAsync(pathfinderId, updatePathfinder, TestClubCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedPathfinder);

            var result = await _controller.PutAsync(pathfinderId, updatePathfinder, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(updatedPathfinder));
        }

        [Test]
        public async Task Put_WithInvalidIds_ReturnsNotFound()
        {
            var pathfinderId = Guid.NewGuid();
            var updatePathfinder = new Incoming.PutPathfinderDto
            {
                Grade = 6,
                IsActive = true
            };

            _pathfinderServiceMock
                .Setup(x => x.UpdateAsync(pathfinderId, updatePathfinder, TestClubCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.PathfinderDto)null);

            var result = await _controller.PutAsync(pathfinderId, updatePathfinder, new CancellationToken());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Put_WithValidationException_Returns400BadRequest()
        {
            var pathfinderId = Guid.NewGuid();
            var updatePathfinder = new Incoming.PutPathfinderDto
            {
                Grade = 15,
                IsActive = true
            };

            var validationErrors = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Grade", "Grade must be between 5 and 12.")
            };
            var validationException = new ValidationException(validationErrors);

            _pathfinderServiceMock
                .Setup(x => x.UpdateAsync(pathfinderId, updatePathfinder, TestClubCode, It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var result = await _controller.PutAsync(pathfinderId, updatePathfinder, new CancellationToken());

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
            
            var validationProblem = badRequestResult.Value as ValidationProblemDetails;
            Assert.That(validationProblem.Errors, Contains.Key("Grade"));
        }

        [Test]
        public async Task BulkPut_WithValidData_ReturnsMultiStatus()
        {
            var pathfinderId = Guid.NewGuid();
            var bulkData = new List<Incoming.BulkPutPathfinderDto>
            {
                new Incoming.BulkPutPathfinderDto
                {
                    Items = new List<Incoming.BulkPutPathfinderItemDto>
                    {
                        new Incoming.BulkPutPathfinderItemDto
                        {
                            PathfinderId = pathfinderId,
                            Grade = 6,
                            IsActive = true
                        }
                    }
                }
            };

            var updatedPathfinder = new Outgoing.PathfinderDto
            {
                PathfinderID = pathfinderId,
                FirstName = "Test",
                LastName = "User",
                Grade = 6,
                IsActive = true
            };

            _pathfinderServiceMock
                .Setup(x => x.BulkUpdateAsync(bulkData, TestClubCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Outgoing.PathfinderDto> { updatedPathfinder });

            var result = await _controller.BulkPutPathfindersAsync(bulkData, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var multiStatusResult = result as ObjectResult;
            Assert.That(multiStatusResult.StatusCode, Is.EqualTo(207));
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