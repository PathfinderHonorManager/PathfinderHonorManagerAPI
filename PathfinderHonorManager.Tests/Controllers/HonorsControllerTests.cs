using System;
using System.Collections.Generic;
using System.Dynamic;
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
using Newtonsoft.Json;

namespace PathfinderHonorManager.Tests.Controllers
{
    public class HonorsControllerTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        private HonorsController _controller;
        private Mock<IHonorService> _honorServiceMock;
        private Mock<ILogger<HonorsController>> _loggerMock;
        private PathfinderContext _dbContext;

        [SetUp]
        public async Task Setup()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            
            _honorServiceMock = new Mock<IHonorService>();
            _loggerMock = new Mock<ILogger<HonorsController>>();
            _controller = new HonorsController(_honorServiceMock.Object, _loggerMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("clubCode", "TEST")
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetHonors_WithValidData_ReturnsOkResult()
        {
            var expectedHonors = new List<Outgoing.HonorDto>
            {
                new Outgoing.HonorDto
                {
                    HonorID = Guid.NewGuid(),
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.png",
                    WikiPath = new Uri("https://example.com")
                }
            };

            _honorServiceMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHonors);

            var result = await _controller.GetHonors(new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedHonors));
        }

        [Test]
        public async Task GetHonors_WithNoHonors_ReturnsNotFound()
        {
            _honorServiceMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<Outgoing.HonorDto>)null);

            var result = await _controller.GetHonors(new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetById_WithValidId_ReturnsOkResult()
        {
            var honorId = Guid.NewGuid();
            var expectedHonor = new Outgoing.HonorDto
            {
                HonorID = honorId,
                Name = "Test Honor",
                Level = 1,
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            _honorServiceMock
                .Setup(x => x.GetByIdAsync(honorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHonor);

            var result = await _controller.GetByIdAsync(honorId, new CancellationToken());

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result;
            var expectedJson = JsonConvert.SerializeObject(new { id = honorId, honor = expectedHonor });
            var actualJson = JsonConvert.SerializeObject(okResult.Value);
            Assert.That(actualJson, Is.EqualTo(expectedJson));
        }

        [Test]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            var honorId = Guid.NewGuid();

            _honorServiceMock
                .Setup(x => x.GetByIdAsync(honorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.HonorDto)null);

            var result = await _controller.GetByIdAsync(honorId, new CancellationToken());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Post_WithValidData_ReturnsCreatedResult()
        {
            var newHonor = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 1,
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            var createdHonor = new Outgoing.HonorDto
            {
                HonorID = Guid.NewGuid(),
                Name = "Test Honor",
                Level = 1,
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            _honorServiceMock
                .Setup(x => x.AddAsync(newHonor, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdHonor);

            var result = await _controller.Post(newHonor, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.InstanceOf<CreatedAtRouteResult>());
            var createdResult = result.Result as CreatedAtRouteResult;
            Assert.That(createdResult.Value, Is.EqualTo(createdHonor));
        }

        [Test]
        public async Task Post_WithValidationError_ReturnsValidationProblem()
        {
            var newHonor = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 0,
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            _honorServiceMock
                .Setup(x => x.AddAsync(newHonor, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation failed"));

            var result = await _controller.Post(newHonor, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result.Result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
        }

        [Test]
        public async Task Post_WithDbError_ReturnsValidationProblem()
        {
            var newHonor = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 1,
                PatchFilename = "test.png",
                WikiPath = new Uri("https://example.com")
            };

            _honorServiceMock
                .Setup(x => x.AddAsync(newHonor, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Database error"));

            var result = await _controller.Post(newHonor, new CancellationToken());

            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result.Result as ObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<ValidationProblemDetails>());
        }

        [Test]
        public async Task Put_WithValidData_ReturnsOkResult()
        {
            var honorId = Guid.NewGuid();
            var updatedHonor = new Incoming.HonorDto
            {
                Name = "Updated Honor",
                Level = 2,
                PatchFilename = "updated.png",
                WikiPath = new Uri("https://example.com/updated")
            };

            var expectedHonor = new Outgoing.HonorDto
            {
                HonorID = honorId,
                Name = "Updated Honor",
                Level = 2,
                PatchFilename = "updated.png",
                WikiPath = new Uri("https://example.com/updated")
            };

            _honorServiceMock
                .Setup(x => x.GetByIdAsync(honorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHonor);

            _honorServiceMock
                .Setup(x => x.UpdateAsync(honorId, updatedHonor, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHonor);

            var result = await _controller.Put(honorId, updatedHonor, new CancellationToken());

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedHonor));
        }

        [Test]
        public async Task Put_WithInvalidId_ReturnsNotFound()
        {
            var honorId = Guid.NewGuid();
            var updatedHonor = new Incoming.HonorDto
            {
                Name = "Updated Honor",
                Level = 2,
                PatchFilename = "updated.png",
                WikiPath = new Uri("https://example.com/updated")
            };

            _honorServiceMock
                .Setup(x => x.GetByIdAsync(honorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Outgoing.HonorDto)null);

            var result = await _controller.Put(honorId, updatedHonor, new CancellationToken());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Put_WithValidationError_ReturnsValidationProblem()
        {
            var honorId = Guid.NewGuid();
            var updatedHonor = new Incoming.HonorDto
            {
                Name = "Updated Honor",
                Level = 0,
                PatchFilename = "updated.png",
                WikiPath = new Uri("https://example.com/updated")
            };

            _honorServiceMock
                .Setup(x => x.GetByIdAsync(honorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Outgoing.HonorDto { HonorID = honorId });

            _honorServiceMock
                .Setup(x => x.UpdateAsync(honorId, updatedHonor, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation failed"));

            var result = await _controller.Put(honorId, updatedHonor, new CancellationToken());

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