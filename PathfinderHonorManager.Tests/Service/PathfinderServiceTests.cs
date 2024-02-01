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
using Moq;
using PathfinderHonorManager.DataAccess;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Service.Interfaces;
using PathfinderHonorManager.Tests.Helpers;

namespace PathfinderHonorManager.Tests.Service
{
    public class PathfinderServiceTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        private PathfinderService _pathfinderService;
        private PathfinderContext _dbContext;

        private Mock<IClubService> _clubServiceMock;
        private List<Pathfinder> _pathfinders;
        private List<Club> _clubs;
        private List<Honor> _honors;
        private List<PathfinderHonor> _pathfinderHonors;
        private PathfinderSelectorHelper _pathfinderSelectorHelper;

        public PathfinderServiceTests()
        {
            _dbContext = new PathfinderContext(SharedContextOptions);
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();

            var validator = new DummyValidator<Incoming.PathfinderDtoInternal>();
            var logger = NullLogger<PathfinderService>.Instance;
            _clubServiceMock = new Mock<IClubService>();

            _clubServiceMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string code, CancellationToken cancellationToken) =>
                {
                    var club = _dbContext.Clubs
                        .Where(c => c.ClubCode == code)
                        .Select(c => new Outgoing.ClubDto
                        {
                            ClubID = c.ClubID,
                            Name = c.Name,
                            ClubCode = c.ClubCode
                        })
                        .FirstOrDefault();

                    return club;
                });
            _pathfinderService = new PathfinderService(_dbContext, _clubServiceMock.Object, mapper, validator, logger);
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
        [SetUp]
        public async Task SetUp()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            _clubs = await _dbContext.Clubs.ToListAsync();
            _pathfinders = await _dbContext.Pathfinders.ToListAsync();
            _honors = await _dbContext.Honors.ToListAsync();
            _pathfinderHonors = await _dbContext.PathfinderHonors.ToListAsync();
            _pathfinderSelectorHelper = new PathfinderSelectorHelper(_pathfinders, _pathfinderHonors);

        }

        [Test]
        [TestCase("VALIDCLUBCODE", true)]
        [TestCase("VALIDCLUBCODE", false)]
        public async Task GetAllAsync_ReturnsAllPathfinders(string clubCode, bool showInactive)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            int expectedCount;
            if (showInactive)
            {
                expectedCount = _pathfinders.Count();
            }
            else
            {
                expectedCount = _pathfinders.Count(p => p.IsActive == true);
            };
            // Act
            var result = await _pathfinderService.GetAllAsync(clubCode, showInactive, cancellationToken);

            // Assert
            Assert.IsNotEmpty(result);
            Assert.IsInstanceOf<IEnumerable<Outgoing.PathfinderDependantDto>>(result);
            Assert.AreEqual(expectedCount, result.Count);
        }

        [TestCase("INVALIDCLUBCODE", true)]
        [TestCase("EMPTYCLUB", true)]
        public async Task GetAllAsync_ReturnsNoPathfindersInvalidOrEmptyClub(string clubCode, bool showInactive)
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderService.GetAllAsync(clubCode, showInactive, cancellationToken);

            // Assert
            Assert.IsEmpty(result);
        }

        [TestCase("VALIDCLUBCODE")]
        public async Task GetByIdAsync_ReturnsPathfinderById(string clubCode)
        {
            // Arrange
            var pathfinderId = _pathfinderSelectorHelper.SelectPathfinderId(true);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderService.GetByIdAsync(pathfinderId, clubCode, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Outgoing.PathfinderDependantDto>(result);
            Assert.AreEqual(pathfinderId, result.PathfinderID);
        }

        [TestCase("VALIDCLUBCODE")]
        public async Task AddAsync_AddsNewPathfinderAndReturnsDto(string clubCode)
        {
            // Arrange
            var newPathfinderDto = new Incoming.PathfinderDto { /* Populate required properties */ };
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderService.AddAsync(newPathfinderDto, clubCode, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Outgoing.PathfinderDto>(result);
            Assert.AreEqual(newPathfinderDto.Email, result.Email);
        }

        [TestCase("VALIDCLUBCODE", 5, true)]
        [TestCase("VALIDCLUBCODE", 12, true)]
        [TestCase("VALIDCLUBCODE", null, true)]
        [TestCase("VALIDCLUBCODE", 5, false)]
        [TestCase("VALIDCLUBCODE", 12, false)]
        [TestCase("VALIDCLUBCODE", null, false)]
        public async Task UpdateAsync_UpdatesPathfinderAndReturnsUpdatedDto(string clubCode, int grade, bool isActive)
        {
            // Arrange
            var pathfinderId = _pathfinderSelectorHelper.SelectPathfinderId(true);
            var cancellationToken = new CancellationToken();
            Incoming.PutPathfinderDto updatePathfinderDto = new Incoming.PutPathfinderDto
            {
                Grade = grade,
                IsActive = isActive
            };

            // Act
            var result = await _pathfinderService.UpdateAsync(pathfinderId, updatePathfinderDto, clubCode, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(updatePathfinderDto.Grade, result.Grade);
            Assert.AreEqual(updatePathfinderDto.IsActive, result.IsActive);
            // Additional assertions as necessary
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