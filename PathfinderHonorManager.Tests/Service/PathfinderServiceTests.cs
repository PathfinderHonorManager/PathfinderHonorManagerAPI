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

        private readonly PathfinderService _pathfinderService;
        private PathfinderContext _dbContext;

        private readonly Mock<IClubService> _clubServiceMock;
        private List<Pathfinder> _pathfinders;
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
            _pathfinders = await _dbContext.Pathfinders.ToListAsync();
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
                expectedCount = _pathfinders.Count;
            }
            else
            {
                expectedCount = _pathfinders.Count(p => p.IsActive == true);
            }
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
            var newPathfinderDto = new Incoming.PathfinderDto 
            { 
                FirstName = "Test",
                LastName = "Pathfinder",
                Email = "test.pathfinder@example.com"
            };
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
            var pathfinder = _pathfinders[0];
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

            // Act
            var results = await _pathfinderService.BulkUpdateAsync(bulkData, clubCode, cancellationToken);

            // Assert
            Assert.That(results, Has.Count.EqualTo(bulkData.Count));
            var resultsList = results.ToList();
            var bulkDataItems = bulkData.Select(b => b.Items.First()).ToList();
            for (int i = 0; i < bulkData.Count; i++)
            {
                Assert.That(resultsList[i].PathfinderID, Is.EqualTo(bulkDataItems[i].PathfinderId));
                Assert.That(resultsList[i].Grade, Is.EqualTo(bulkDataItems[i].Grade));
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

        [Test]
        public async Task UpdateAsync_WithValidData_ReturnsUpdatedPathfinder()
        {
            // Arrange
            var token = new CancellationToken();
            var pathfinder = _pathfinders.First(p => p.IsActive == true);
            var updatedPathfinder = new Incoming.PutPathfinderDto
            {
                Grade = 8,
                IsActive = true
            };

            // Act
            var result = await _pathfinderService.UpdateAsync(pathfinder.PathfinderID, updatedPathfinder, "VALIDCLUBCODE", token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PathfinderID, Is.EqualTo(pathfinder.PathfinderID));
            Assert.That(result.Grade, Is.EqualTo(updatedPathfinder.Grade));
            Assert.That(result.IsActive, Is.EqualTo(updatedPathfinder.IsActive));
            Assert.That(result.FirstName, Is.EqualTo(pathfinder.FirstName));
            Assert.That(result.LastName, Is.EqualTo(pathfinder.LastName));
            Assert.That(result.Email, Is.EqualTo(pathfinder.Email));

            // Verify database update
            var dbPathfinder = await _dbContext.Pathfinders.AsNoTracking().FirstOrDefaultAsync(p => p.PathfinderID == pathfinder.PathfinderID);
            Assert.That(dbPathfinder, Is.Not.Null);
            Assert.That(dbPathfinder.Grade, Is.EqualTo(updatedPathfinder.Grade));
            Assert.That(dbPathfinder.IsActive, Is.EqualTo(updatedPathfinder.IsActive));
            Assert.That(dbPathfinder.FirstName, Is.EqualTo(pathfinder.FirstName));
            Assert.That(dbPathfinder.LastName, Is.EqualTo(pathfinder.LastName));
            Assert.That(dbPathfinder.Email, Is.EqualTo(pathfinder.Email));
        }

        [Test]
        public async Task UpdateAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var token = new CancellationToken();
            var invalidId = Guid.NewGuid();
            var updatedPathfinder = new Incoming.PutPathfinderDto
            {
                Grade = 8,
                IsActive = true
            };

            // Act
            var result = await _pathfinderService.UpdateAsync(invalidId, updatedPathfinder, "VALIDCLUBCODE", token);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateAsync_WithInvalidClubCode_ReturnsNull()
        {
            // Arrange
            var token = new CancellationToken();
            var pathfinder = _pathfinders.First(p => p.IsActive == true);
            var updatedPathfinder = new Incoming.PutPathfinderDto
            {
                Grade = 8,
                IsActive = true
            };

            // Act
            var result = await _pathfinderService.UpdateAsync(pathfinder.PathfinderID, updatedPathfinder, "INVALIDCLUBCODE", token);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task BulkUpdateAsync_WithValidData_ReturnsUpdatedPathfinders()
        {
            // Arrange
            var token = new CancellationToken();
            var pathfinders = _pathfinders.Where(p => p.IsActive == true).Take(2).ToList();
            var bulkData = new List<Incoming.BulkPutPathfinderDto>
            {
                new Incoming.BulkPutPathfinderDto
                {
                    Items = pathfinders.Select(p => new Incoming.BulkPutPathfinderItemDto
                    {
                        PathfinderId = p.PathfinderID,
                        Grade = 8,
                        IsActive = true
                    }).ToList()
                }
            };

            // Act
            var result = await _pathfinderService.BulkUpdateAsync(bulkData, "VALIDCLUBCODE", token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(pathfinders.Count));
            foreach (var updatedPathfinder in result)
            {
                var originalPathfinder = pathfinders.First(p => p.PathfinderID == updatedPathfinder.PathfinderID);
                Assert.That(updatedPathfinder.Grade, Is.EqualTo(8));
                Assert.That(updatedPathfinder.IsActive, Is.EqualTo(true));
                Assert.That(updatedPathfinder.FirstName, Is.EqualTo(originalPathfinder.FirstName));
                Assert.That(updatedPathfinder.LastName, Is.EqualTo(originalPathfinder.LastName));
                Assert.That(updatedPathfinder.Email, Is.EqualTo(originalPathfinder.Email));
            }

            // Verify database updates
            foreach (var pathfinder in pathfinders)
            {
                var dbPathfinder = await _dbContext.Pathfinders.AsNoTracking().FirstOrDefaultAsync(p => p.PathfinderID == pathfinder.PathfinderID);
                Assert.That(dbPathfinder, Is.Not.Null);
                Assert.That(dbPathfinder.Grade, Is.EqualTo(8));
                Assert.That(dbPathfinder.IsActive, Is.EqualTo(true));
                Assert.That(dbPathfinder.FirstName, Is.EqualTo(pathfinder.FirstName));
                Assert.That(dbPathfinder.LastName, Is.EqualTo(pathfinder.LastName));
                Assert.That(dbPathfinder.Email, Is.EqualTo(pathfinder.Email));
            }
        }

        [Test]
        public async Task BulkUpdateAsync_WithInvalidIds_ReturnsEmptyList()
        {
            // Arrange
            var token = new CancellationToken();
            var invalidId = Guid.NewGuid();
            var bulkData = new List<Incoming.BulkPutPathfinderDto>
            {
                new Incoming.BulkPutPathfinderDto
                {
                    Items = new List<Incoming.BulkPutPathfinderItemDto>
                    {
                        new Incoming.BulkPutPathfinderItemDto
                        {
                            PathfinderId = invalidId,
                            Grade = 8,
                            IsActive = true
                        }
                    }
                }
            };

            // Act
            var result = await _pathfinderService.BulkUpdateAsync(bulkData, "VALIDCLUBCODE", token);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task BulkUpdateAsync_WithInvalidClubCode_SavesChanges()
        {
            // Arrange
            var token = new CancellationToken();
            var pathfinders = _pathfinders.Where(p => p.IsActive == true).Take(2).ToList();
            var bulkData = new List<Incoming.BulkPutPathfinderDto>
            {
                new Incoming.BulkPutPathfinderDto
                {
                    Items = pathfinders.Select(p => new Incoming.BulkPutPathfinderItemDto
                    {
                        PathfinderId = p.PathfinderID,
                        Grade = 8,
                        IsActive = true
                    }).ToList()
                }
            };

            // Act
            var result = await _pathfinderService.BulkUpdateAsync(bulkData, "INVALIDCLUBCODE", token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(pathfinders.Count));
            foreach (var updatedPathfinder in result)
            {
                var originalPathfinder = pathfinders.First(p => p.PathfinderID == updatedPathfinder.PathfinderID);
                Assert.That(updatedPathfinder.Grade, Is.EqualTo(8));
                Assert.That(updatedPathfinder.IsActive, Is.EqualTo(true));
                Assert.That(updatedPathfinder.FirstName, Is.EqualTo(originalPathfinder.FirstName));
                Assert.That(updatedPathfinder.LastName, Is.EqualTo(originalPathfinder.LastName));
                Assert.That(updatedPathfinder.Email, Is.EqualTo(originalPathfinder.Email));
            }

            // Verify database updates
            foreach (var pathfinder in pathfinders)
            {
                var dbPathfinder = await _dbContext.Pathfinders.AsNoTracking().FirstOrDefaultAsync(p => p.PathfinderID == pathfinder.PathfinderID);
                Assert.That(dbPathfinder, Is.Not.Null);
                Assert.That(dbPathfinder.Grade, Is.EqualTo(8));
                Assert.That(dbPathfinder.IsActive, Is.EqualTo(true));
                Assert.That(dbPathfinder.FirstName, Is.EqualTo(pathfinder.FirstName));
                Assert.That(dbPathfinder.LastName, Is.EqualTo(pathfinder.LastName));
                Assert.That(dbPathfinder.Email, Is.EqualTo(pathfinder.Email));
            }
        }

        [Test]
        public void AddAsync_WithValidationException_ThrowsValidationException()
        {
            var newPathfinderDto = new Incoming.PathfinderDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Grade = 5
            };

            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Pathfinder email address (test@example.com) is taken.")
            };
            var validationException = new ValidationException(validationErrors);

            var validatorMock = new Mock<IValidator<Incoming.PathfinderDtoInternal>>();
            validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Incoming.PathfinderDtoInternal>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            var logger = NullLogger<PathfinderService>.Instance;
            
            var pathfinderService = new PathfinderService(_dbContext, _clubServiceMock.Object, mapper, validatorMock.Object, logger);

            var ex = Assert.ThrowsAsync<ValidationException>(
                async () => await pathfinderService.AddAsync(newPathfinderDto, "VALIDCLUBCODE", CancellationToken.None));

            Assert.That(ex.Errors.First().PropertyName, Is.EqualTo("Email"));
            Assert.That(ex.Errors.First().ErrorMessage, Is.EqualTo("Pathfinder email address (test@example.com) is taken."));
        }

        [Test]
        public void UpdateAsync_WithValidationException_ThrowsValidationException()
        {
            var pathfinderId = _pathfinderSelectorHelper.SelectPathfinderId(true);
            var updatePathfinderDto = new Incoming.PutPathfinderDto
            {
                Grade = 15,
                IsActive = true
            };

            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Grade", "Grade must be between 5 and 12.")
            };
            var validationException = new ValidationException(validationErrors);

            var validatorMock = new Mock<IValidator<Incoming.PathfinderDtoInternal>>();
            validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Incoming.PathfinderDtoInternal>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            var logger = NullLogger<PathfinderService>.Instance;
            
            var pathfinderService = new PathfinderService(_dbContext, _clubServiceMock.Object, mapper, validatorMock.Object, logger);

            var ex = Assert.ThrowsAsync<ValidationException>(
                async () => await pathfinderService.UpdateAsync(pathfinderId, updatePathfinderDto, "VALIDCLUBCODE", CancellationToken.None));

            Assert.That(ex.Errors.First().PropertyName, Is.EqualTo("Grade"));
            Assert.That(ex.Errors.First().ErrorMessage, Is.EqualTo("Grade must be between 5 and 12."));
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