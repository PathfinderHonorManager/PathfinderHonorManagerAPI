using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Incoming;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Tests.Helpers;

namespace PathfinderHonorManager.Tests.Service
{
    public class PathfinderHonorServiceTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        private PathfinderHonorService _pathfinderHonorService;
        private PathfinderContext _dbContext;

        private List<Pathfinder> _pathfinders;
        private List<Honor> _honors;
        private List<PathfinderHonor> _pathfinderHonors;
        private PathfinderSelectorHelper _pathfinderSelectorHelper;


        public PathfinderHonorServiceTests()
        {
            _dbContext = new PathfinderContext(SharedContextOptions);

        }


        [SetUp]
        public async Task SetUp()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);

            _dbContext = new PathfinderContext(SharedContextOptions);

            _pathfinders = await _dbContext.Pathfinders.ToListAsync();
            _honors = await _dbContext.Honors.ToListAsync();
            _pathfinderHonors = await _dbContext.PathfinderHonors.ToListAsync();
            _pathfinderSelectorHelper = new PathfinderSelectorHelper(_pathfinders, _pathfinderHonors);
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();

            var logger = new NullLogger<PathfinderHonorService>();

            var validator = new DummyValidator<PathfinderHonorDto>();

            _pathfinderHonorService = new PathfinderHonorService(_dbContext, mapper, validator, logger);

        }

        [Test]
        [TestCase("planned")]
        [TestCase("earned")]
        [TestCase("awarded")]
        public async Task GetAllByStatusAsync_ReturnsPathfinderHonorsForStatus(string status)
        {
            // Act
            CancellationToken token = new();
            var result = await _pathfinderHonorService.GetAllByStatusAsync(status, token);
            // Assert using fluent assertions
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.All(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task GetByIdAsync_ReturnsPathfinderHonorsForPathfinderIdAndHonorId(int id)
        {
            // Act
            CancellationToken token = new();
            var pathfinderId = _pathfinderSelectorHelper.SelectPathfinderId(true);
            var result = await _pathfinderHonorService.GetByIdAsync(pathfinderId, _pathfinderHonors[id].HonorID, token);

            // Assert using fluent assertions
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PathfinderID, Is.EqualTo(pathfinderId));
            Assert.That(result.HonorID, Is.EqualTo(_pathfinderHonors[id].HonorID));
        }

        [TestCase(0, "planned")]
        [TestCase(1, "earned")]
        [TestCase(2, "awarded")]
        public async Task AddAsync_AddsNewPathfinderHonorAndReturnsDto(int honorIndex, string honorStatus)
        {
            // Arrange
            var postPathfinderHonorDto = new PostPathfinderHonorDto
            {
                HonorID = _honors[honorIndex].HonorID,
                Status = honorStatus.ToString()
            };
            CancellationToken token = new();

            // Act
            var pathfinderId = _pathfinderSelectorHelper.SelectPathfinderId(false);
            var result = await _pathfinderHonorService.AddAsync(pathfinderId, postPathfinderHonorDto, token);

            // Assert using fluent assertions
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PathfinderID, Is.EqualTo(pathfinderId));
            Assert.That(result.HonorID, Is.EqualTo(_honors[honorIndex].HonorID));
            Assert.That(result.Status, Is.EqualTo(honorStatus).IgnoreCase);
        }

        [TestCase(0, 0, "awarded")]
        [TestCase(1, 1, "earned")]
        [TestCase(2, 2, "planned")]
        public async Task UpdateAsync_UpdatesPathfinderHonorAndReturnsUpdatedDto(int honorIndex, int pathfinderHonorIndex, string honorStatus)
        {
            // Arrange
            var putPathfinderHonorDto = new PutPathfinderHonorDto
            {
                Status = honorStatus.ToString()
            };
            CancellationToken token = new();

            // Act
            var pathfinderId = _pathfinderSelectorHelper.SelectPathfinderId(true);
            var result = await _pathfinderHonorService.UpdateAsync(pathfinderId, _honors[honorIndex].HonorID, putPathfinderHonorDto, token);

            // Assert using fluent assertions
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PathfinderID, Is.EqualTo(pathfinderId));
            Assert.That(result.HonorID, Is.EqualTo(_honors[honorIndex].HonorID));
            Assert.That(result.Status, Is.EqualTo(honorStatus).IgnoreCase);
        }

        [TearDown]
        public async Task TearDown()
        {
            await DatabaseCleaner.CleanDatabase(_dbContext);
            _dbContext.Dispose();
        }


        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _dbContext.Dispose();
        }
    }
}
