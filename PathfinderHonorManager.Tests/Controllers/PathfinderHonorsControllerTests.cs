using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
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
    public class PathfinderHonorsControllerTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        private PathfinderHonorsController _controller;
        private Mock<IPathfinderHonorService> _pathfinderHonorServiceMock;
        private PathfinderContext _dbContext;

        [SetUp]
        public async Task Setup()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            
            _pathfinderHonorServiceMock = new Mock<IPathfinderHonorService>();
            _controller = new PathfinderHonorsController(_pathfinderHonorServiceMock.Object);
        }

        [Test]
        public async Task GetAll_WithValidPathfinderId_ReturnsOkResult()
        {
            var pathfinderId = Guid.NewGuid();
            var expectedHonors = new List<Outgoing.PathfinderHonorDto>
            {
                new Outgoing.PathfinderHonorDto
                {
                    PathfinderID = pathfinderId,
                    HonorID = Guid.NewGuid(),
                    Status = "planned"
                }
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.GetAllAsync(pathfinderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHonors);

            var result = await _controller.GetAll(pathfinderId, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedHonors));
        }

        [Test]
        public async Task GetAll_WithInvalidPathfinderId_ReturnsNotFound()
        {
            var pathfinderId = Guid.NewGuid();

            _pathfinderHonorServiceMock
                .Setup(x => x.GetAllAsync(pathfinderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<Outgoing.PathfinderHonorDto>)null);

            var result = await _controller.GetAll(pathfinderId, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetAllByStatus_WithValidStatus_ReturnsOkResult()
        {
            var status = "planned";
            var expectedHonors = new List<Outgoing.PathfinderHonorDto>
            {
                new Outgoing.PathfinderHonorDto
                {
                    PathfinderID = Guid.NewGuid(),
                    HonorID = Guid.NewGuid(),
                    Status = status
                }
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.GetAllByStatusAsync(status, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHonors);

            var result = await _controller.GetAllByStatus(status, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedHonors));
        }

        [Test]
        public async Task GetAllByStatus_WithInvalidStatus_ReturnsNotFound()
        {
            var status = "invalid";

            _pathfinderHonorServiceMock
                .Setup(x => x.GetAllByStatusAsync(status, It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<Outgoing.PathfinderHonorDto>)null);

            var result = await _controller.GetAllByStatus(status, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetById_WithValidIds_ReturnsOkResult()
        {
            var pathfinderId = Guid.NewGuid();
            var honorId = Guid.NewGuid();
            var expectedHonor = new Outgoing.PathfinderHonorDto
            {
                PathfinderID = pathfinderId,
                HonorID = honorId,
                Status = "planned"
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.GetByIdAsync(pathfinderId, honorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHonor);

            var result = await _controller.GetByIdAsync(pathfinderId, honorId, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedHonor));
        }

        [Test]
        public async Task GetById_WithInvalidIds_ReturnsNotFound()
        {
            var pathfinderId = Guid.NewGuid();
            var honorId = Guid.NewGuid();

            _pathfinderHonorServiceMock
                .Setup(x => x.GetByIdAsync(pathfinderId, honorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.PathfinderHonorDto)null);

            var result = await _controller.GetByIdAsync(pathfinderId, honorId, new CancellationToken());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Post_WithValidData_ReturnsCreatedAtRouteWithCorrectRouteValues()
        {
            var pathfinderId = Guid.NewGuid();
            var honorId = Guid.NewGuid();
            var newHonor = new Incoming.PostPathfinderHonorDto
            {
                HonorID = honorId,
                Status = "planned"
            };

            var createdHonor = new Outgoing.PathfinderHonorDto
            {
                PathfinderID = pathfinderId,
                HonorID = honorId,
                Status = "planned"
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.AddAsync(pathfinderId, newHonor, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdHonor);

            var result = await _controller.PostAsync(pathfinderId, newHonor, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<CreatedAtRouteResult>());
            
            var createdResult = result as CreatedAtRouteResult;
            Assert.That(createdResult.Value, Is.EqualTo(createdHonor));
            Assert.That(createdResult.RouteName, Is.EqualTo("GetPathfinderHonorById"));
            Assert.That(createdResult.RouteValues, Is.Not.Null);
            Assert.That(createdResult.RouteValues["pathfinderId"], Is.EqualTo(pathfinderId));
            Assert.That(createdResult.RouteValues["honorId"], Is.EqualTo(honorId));
        }

        [Test]
        public async Task Post_WithValidationError_ReturnsBadRequest()
        {
            var pathfinderId = Guid.NewGuid();
            var newHonor = new Incoming.PostPathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                Status = "planned"
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.AddAsync(pathfinderId, newHonor, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation failed"));

            var result = await _controller.PostAsync(pathfinderId, newHonor, new CancellationToken());

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
        }

        [Test]
        public async Task Post_WithDbError_ReturnsBadRequest()
        {
            var pathfinderId = Guid.NewGuid();
            var newHonor = new Incoming.PostPathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                Status = "planned"
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.AddAsync(pathfinderId, newHonor, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Database error"));

            var result = await _controller.PostAsync(pathfinderId, newHonor, new CancellationToken());

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
        }

        [Test]
        public async Task BulkPost_WithValidData_ReturnsMultiStatus()
        {
            var pathfinderId = Guid.NewGuid();
            var bulkData = new List<Incoming.BulkPostPathfinderHonorDto>
            {
                new Incoming.BulkPostPathfinderHonorDto
                {
                    PathfinderID = pathfinderId,
                    Honors = new List<Incoming.PostPathfinderHonorDto>
                    {
                        new Incoming.PostPathfinderHonorDto
                        {
                            HonorID = Guid.NewGuid(),
                            Status = "planned"
                        }
                    }
                }
            };

            var createdHonor = new Outgoing.PathfinderHonorDto
            {
                PathfinderID = pathfinderId,
                HonorID = bulkData[0].Honors.First().HonorID,
                Status = "planned"
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.AddAsync(pathfinderId, It.IsAny<Incoming.PostPathfinderHonorDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdHonor);

            var result = await _controller.BulkPostAsync(bulkData, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var multiStatusResult = result as ObjectResult;
            Assert.That(multiStatusResult.StatusCode, Is.EqualTo(207));
        }

        [Test]
        public async Task Put_WithValidData_ReturnsOkResult()
        {
            var pathfinderId = Guid.NewGuid();
            var honorId = Guid.NewGuid();
            var updateHonor = new Incoming.PutPathfinderHonorDto
            {
                Status = "earned"
            };

            var updatedHonor = new Outgoing.PathfinderHonorDto
            {
                PathfinderID = pathfinderId,
                HonorID = honorId,
                Status = "earned"
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.UpdateAsync(pathfinderId, honorId, updateHonor, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedHonor);

            var result = await _controller.PutAsync(pathfinderId, honorId, updateHonor, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(updatedHonor));
        }

        [Test]
        public async Task Put_WithInvalidIds_ReturnsNotFound()
        {
            var pathfinderId = Guid.NewGuid();
            var honorId = Guid.NewGuid();
            var updateHonor = new Incoming.PutPathfinderHonorDto
            {
                Status = "earned"
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.UpdateAsync(pathfinderId, honorId, updateHonor, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.PathfinderHonorDto)null);

            var result = await _controller.PutAsync(pathfinderId, honorId, updateHonor, new CancellationToken());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Put_WithValidationError_ReturnsBadRequest()
        {
            var pathfinderId = Guid.NewGuid();
            var honorId = Guid.NewGuid();
            var updateHonor = new Incoming.PutPathfinderHonorDto
            {
                Status = "invalid"
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.UpdateAsync(pathfinderId, honorId, updateHonor, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation failed"));

            var result = await _controller.PutAsync(pathfinderId, honorId, updateHonor, new CancellationToken());

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
        }

        [Test]
        public async Task BulkPut_WithValidData_ReturnsMultiStatus()
        {
            var pathfinderId = Guid.NewGuid();
            var honorId = Guid.NewGuid();
            var bulkData = new List<Incoming.BulkPutPathfinderHonorDto>
            {
                new Incoming.BulkPutPathfinderHonorDto
                {
                    PathfinderID = pathfinderId,
                    Honors = new List<Incoming.PutPathfinderHonorDto>
                    {
                        new Incoming.PutPathfinderHonorDto
                        {
                            HonorID = honorId,
                            Status = "earned"
                        }
                    }
                }
            };

            var updatedHonor = new Outgoing.PathfinderHonorDto
            {
                PathfinderID = pathfinderId,
                HonorID = honorId,
                Status = "earned"
            };

            _pathfinderHonorServiceMock
                .Setup(x => x.UpdateAsync(pathfinderId, honorId, It.IsAny<Incoming.PutPathfinderHonorDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedHonor);

            var result = await _controller.BulkPutAsync(bulkData, new CancellationToken());

            Assert.That(result, Is.Not.Null);
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