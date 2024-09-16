using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
        private List<PathfinderAchievement> _pathfinderAchievements;
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

        [SetUp]
        public async Task SetUp()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            _clubs = await _dbContext.Clubs.ToListAsync();
            _pathfinders = await _dbContext.Pathfinders.ToListAsync();
            _honors = await _dbContext.Honors.ToListAsync();
            _pathfinderHonors = await _dbContext.PathfinderHonors.ToListAsync();
            _pathfinderAchievements = await _dbContext.PathfinderAchievements
                                                .Include(a => a.Achievement)
                                                .ToListAsync();
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
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Is.InstanceOf<IEnumerable<Outgoing.PathfinderDependantDto>>());
            Assert.That(result.Count, Is.EqualTo(expectedCount));
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
            Assert.That(result, Is.Empty);
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
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Outgoing.PathfinderDependantDto>());
            Assert.That(result.PathfinderID, Is.EqualTo(pathfinderId));
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
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Outgoing.PathfinderDto>());
            Assert.That(result.Email, Is.EqualTo(newPathfinderDto.Email));
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
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Grade, Is.EqualTo(updatePathfinderDto.Grade));
            Assert.That(result.IsActive, Is.EqualTo(updatePathfinderDto.IsActive));
        }

        [TestCase("VALIDCLUBCODE")]
        public async Task GetPathfinderAchievementDetailsAsync_ReturnsAccurateCounts(string clubCode)
        {
            // Arrange
            var pathfinder = _pathfinders.First();
            var expectedAssignedBasicAchievementCount = _pathfinderAchievements.Count(pa => pa.PathfinderID == pathfinder.PathfinderID && pa.Achievement.Grade == pathfinder.Grade && pa.Achievement.Level == 1); 
            var expectedCompletedBasicAchievementCount = _pathfinderAchievements.Count(pa => pa.PathfinderID == pathfinder.PathfinderID && pa.Achievement.Grade == pathfinder.Grade && pa.Achievement.Level == 1 && pa.IsAchieved);
            var expectedAssignedAdvancedAchievementCount = _pathfinderAchievements.Count(pa => pa.PathfinderID == pathfinder.PathfinderID && pa.Achievement.Grade == pathfinder.Grade && pa.Achievement.Level == 2);
            var expectedCompletedAdvancedAchievementCount = _pathfinderAchievements.Count(pa => pa.PathfinderID == pathfinder.PathfinderID && pa.Achievement.Grade == pathfinder.Grade && pa.Achievement.Level == 2 && pa.IsAchieved);

            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderService.GetByIdAsync(pathfinder.PathfinderID, clubCode, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AssignedBasicAchievementsCount, Is.EqualTo(expectedAssignedBasicAchievementCount), "Assigned basic achievement count should match expected value.");
            Assert.That(result.CompletedBasicAchievementsCount, Is.EqualTo(expectedCompletedBasicAchievementCount), "Completed basic achievement count should match expected value.");
            Assert.That(result.AssignedAdvancedAchievementsCount, Is.EqualTo(expectedAssignedAdvancedAchievementCount), "Assigned advanced achievement count should match expected value.");
            Assert.That(result.CompletedAdvancedAchievementsCount, Is.EqualTo(expectedCompletedAdvancedAchievementCount), "Completed advanced achievement count should match expected value.");
        }

        [TestCase("VALIDCLUBCODE")]
        public async Task GetAllPathfindersAchievementDetailsAsync_ReturnsAccurateCountsForEachGrade(string clubCode)
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            ICollection<PathfinderDependantDto> allPathfinders = await _pathfinderService.GetAllAsync(clubCode, true, cancellationToken);

            // Assert
            foreach (var pathfinder in allPathfinders)
            {
                var expectedAssignedBasicAchievementCount = _pathfinderAchievements.Count(pa => pa.PathfinderID == pathfinder.PathfinderID && pa.Achievement.Grade == pathfinder.Grade && pa.Achievement.Level == 1);
                var expectedCompletedBasicAchievementCount = _pathfinderAchievements.Count(pa => pa.PathfinderID == pathfinder.PathfinderID && pa.Achievement.Grade == pathfinder.Grade && pa.Achievement.Level == 1 && pa.IsAchieved);
                var expectedAssignedAdvancedAchievementCount = _pathfinderAchievements.Count(pa => pa.PathfinderID == pathfinder.PathfinderID && pa.Achievement.Grade == pathfinder.Grade && pa.Achievement.Level == 2);
                var expectedCompletedAdvancedAchievementCount = _pathfinderAchievements.Count(pa => pa.PathfinderID == pathfinder.PathfinderID && pa.Achievement.Grade == pathfinder.Grade && pa.Achievement.Level == 2 && pa.IsAchieved);

                Assert.That(pathfinder.AssignedBasicAchievementsCount, Is.EqualTo(expectedAssignedBasicAchievementCount), "Assigned basic achievement count should match expected value.");
                Assert.That(pathfinder.CompletedBasicAchievementsCount, Is.EqualTo(expectedCompletedBasicAchievementCount), "Completed basic achievement count should match expected value.");
                Assert.That(pathfinder.AssignedAdvancedAchievementsCount, Is.EqualTo(expectedAssignedAdvancedAchievementCount), "Assigned advanced achievement count should match expected value.");
                Assert.That(pathfinder.CompletedAdvancedAchievementsCount, Is.EqualTo(expectedCompletedAdvancedAchievementCount), "Completed advanced achievement count should match expected value.");
            }
        }

        [TestCase("VALIDCLUBCODE")]
        public async Task BulkUpdateAsync_UpdatesMultiplePathfindersAndReturnsUpdatedDtos(string clubCode)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var bulkData = new List<Incoming.BulkPutPathfinderDto>();
            var pathfinderIds = _pathfinderSelectorHelper.SelectUniquePathfinderIds(3);

            // Act
            var results = await _pathfinderService.BulkUpdateAsync(bulkData, clubCode, cancellationToken);

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(bulkData.Count));
            for (int i = 0; i < bulkData.Count; i++)
            {
                Assert.That(results.ElementAt(i).PathfinderID, Is.EqualTo(bulkData.ElementAt(i).Items.First().PathfinderId));
                Assert.That(results.ElementAt(i).Grade, Is.EqualTo(bulkData.ElementAt(i).Items.First().Grade));
            }

            // Verify that the changes were persisted in the database
            var updatedPathfinders = await _dbContext.Pathfinders
                .Where(p => bulkData.Select(b => b.Items.First().PathfinderId).Contains(p.PathfinderID))
                .ToListAsync(cancellationToken);

            foreach (var updatedPathfinder in updatedPathfinders)
            {
                var correspondingBulkData = bulkData.First(b => b.Items.First().PathfinderId == updatedPathfinder.PathfinderID);
                Assert.That(updatedPathfinder.Grade, Is.EqualTo(correspondingBulkData.Items.First().Grade));
            }
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