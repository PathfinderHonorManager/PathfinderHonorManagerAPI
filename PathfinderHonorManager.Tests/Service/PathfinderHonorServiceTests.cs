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
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase)));
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

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(pathfinderId, result.PathfinderID);
            Assert.AreEqual(_pathfinderHonors[id].HonorID, result.HonorID);
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

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(pathfinderId, result.PathfinderID);
            Assert.AreEqual(_honors[honorIndex].HonorID, result.HonorID);
            Assert.IsTrue(result.Status.Equals(honorStatus, StringComparison.OrdinalIgnoreCase));
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

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(pathfinderId, result.PathfinderID);
            Assert.AreEqual(_honors[honorIndex].HonorID, result.HonorID);
            Assert.IsTrue(result.Status.Equals(honorStatus, StringComparison.OrdinalIgnoreCase));
        }

        private class DummyValidator<T> : AbstractValidator<T>
        {
            public override ValidationResult Validate(ValidationContext<T> context)
            {
                return new ValidationResult(new List<ValidationFailure>());
            }

            public override Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new ValidationResult(new List<ValidationFailure>()));
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_dbContext != null)
            {
                if (_dbContext.Pathfinders.Any())
                    _dbContext.Pathfinders.RemoveRange(_dbContext.Pathfinders);
                if (_dbContext.Honors.Any())
                    _dbContext.Honors.RemoveRange(_dbContext.Honors);
                if (_dbContext.PathfinderHonors.Any())
                    _dbContext.PathfinderHonors.RemoveRange(_dbContext.PathfinderHonors);
                if (_dbContext.PathfinderHonorStatuses.Any())
                    _dbContext.PathfinderHonorStatuses.RemoveRange(_dbContext.PathfinderHonorStatuses);
                if (_dbContext.Clubs.Any())
                    _dbContext.Clubs.RemoveRange(_dbContext.Clubs);

                _dbContext.SaveChanges();
            }

            _dbContext?.Dispose();

        }


        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _dbContext.Dispose();
        }
    }
}
