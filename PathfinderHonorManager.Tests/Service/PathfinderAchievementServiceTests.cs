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
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Tests.Helpers;
using PathfinderHonorManager.Dto.Outgoing;
using FluentAssertions;

namespace PathfinderHonorManager.Tests.Service
{
    public class PathfinderAchievementServiceTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        private List<Achievement> _achievements;
        private List<Pathfinder> _pathfinders;
        private List<PathfinderAchievement> _pathfinderAchievements;
        private PathfinderContext _dbContext;
        private PathfinderAchievementService _pathfinderAchievementService;

        [SetUp]
        public async Task Setup()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();

            var logger = NullLogger<PathfinderAchievementService>.Instance;
            var validator = new Mock<IValidator<Incoming.PathfinderAchievementDto>>();

            _pathfinderAchievementService = new PathfinderAchievementService(_dbContext, mapper, logger, validator.Object);
            _achievements = await _dbContext.Achievements.ToListAsync();
            _pathfinders = await _dbContext.Pathfinders.ToListAsync();
            _pathfinderAchievements = await _dbContext.PathfinderAchievements.ToListAsync();
        }

        [TestCase]
        public async Task GetAllAsync_ReturnsAllAchievements()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedCount = _pathfinderAchievements.Count;

            // Act
            var result = await _pathfinderAchievementService.GetAllAsync(true, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ICollection<PathfinderAchievementDto>>());
            Assert.That(result.Count, Is.EqualTo(expectedCount));
        }

        [TestCase]
        public async Task GetAllAsync_ReturnsAchievementsInCorrectOrder()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.GetAllAsync(true, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));

            // Verify ordering: PathfinderID, then Grade, then CategorySequenceOrder, then Level, then AchievementSequenceOrder
            var orderedResult = result.ToList();
            for (int i = 1; i < orderedResult.Count; i++)
            {
                var previous = orderedResult[i - 1];
                var current = orderedResult[i];

                // If pathfinders are different, ordering is correct
                if (previous.PathfinderID != current.PathfinderID)
                    continue;

                // If grades are different, current should be >= previous
                if (previous.Grade != current.Grade)
                {
                    Assert.That(current.Grade, Is.GreaterThanOrEqualTo(previous.Grade), 
                        "Achievements should be ordered by Grade within the same pathfinder");
                    continue;
                }

                // If category sequence orders are different, current should be >= previous
                if (previous.CategorySequenceOrder != current.CategorySequenceOrder)
                {
                    Assert.That(current.CategorySequenceOrder, Is.GreaterThanOrEqualTo(previous.CategorySequenceOrder), 
                        "Achievements should be ordered by CategorySequenceOrder within the same grade");
                    continue;
                }

                // If levels are different, current should be >= previous
                if (previous.Level != current.Level)
                {
                    Assert.That(current.Level, Is.GreaterThanOrEqualTo(previous.Level), 
                        "Achievements should be ordered by Level within the same category");
                    continue;
                }

                // If achievement sequence orders are different, current should be >= previous
                if (previous.AchievementSequenceOrder != current.AchievementSequenceOrder)
                {
                    Assert.That(current.AchievementSequenceOrder, Is.GreaterThanOrEqualTo(previous.AchievementSequenceOrder), 
                        "Achievements should be ordered by AchievementSequenceOrder within the same level");
                }
            }
        }

        [TestCase]
        public async Task GetByIdAsync_ValidId_ReturnsAchievement()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedPathfinderAchievement = _pathfinderAchievements.First();
            var expectedAchievementId = expectedPathfinderAchievement.AchievementID;
            var expectedPathfinderId = expectedPathfinderAchievement.PathfinderID;

            // Act
            var result = await _pathfinderAchievementService.GetByIdAsync(expectedPathfinderId, expectedAchievementId, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AchievementID, Is.EqualTo(expectedAchievementId));
            Assert.That(result.PathfinderID, Is.EqualTo(expectedPathfinderId));
            Assert.That(result, Is.InstanceOf<PathfinderAchievementDto>());
        }

        [TestCase]
        public async Task AddAsync_AddsNewPathfinderAchievementAndReturnsDto()
        {
            // Arrange
            var pathfinderWithFewestAchievements = _pathfinderAchievements
                .GroupBy(pa => pa.PathfinderID)
                .OrderBy(g => g.Count())
                .First().Key;

            var pathfinderId = pathfinderWithFewestAchievements;
            var achievementsForPathfinder = _pathfinderAchievements
                .Where(pa => pa.PathfinderID == pathfinderId)
                .Select(pa => pa.AchievementID)
                .ToList();

            var newAchievementId = _achievements
                .Where(a => !achievementsForPathfinder.Contains(a.AchievementID))
                .Select(a => a.AchievementID)
                .FirstOrDefault();

            var newAchievementDto = new Incoming.PostPathfinderAchievementDto
            {
                AchievementID = newAchievementId
            };
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.AddAsync(pathfinderId, newAchievementDto, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PathfinderID, Is.EqualTo(pathfinderId));
            Assert.That(result.AchievementID, Is.EqualTo(newAchievementDto.AchievementID));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task UpdateAsync_UpdatesPathfinderAchievementAndReturnsUpdatedDto(bool isAchieved)
        {
            // Arrange
            var pathfinderAchievement = _pathfinderAchievements.First();
            var updateDto = new Incoming.PutPathfinderAchievementDto
            {
                IsAchieved = isAchieved
            };
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.UpdateAsync(pathfinderAchievement.PathfinderID, pathfinderAchievement.AchievementID, updateDto, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsAchieved, Is.EqualTo(isAchieved));
        }

        [Test]
        public async Task AddAchievementsForPathfinderAsync_AddsAchievementsBasedOnGrade()
        {
            // Arrange
            var pathfinderId = _pathfinders.First().PathfinderID;
            var grade = _pathfinders.First().Grade;
            var expectedAchievementsCount = _achievements.Count(a => a.Grade == grade);
            var cancellationToken = new CancellationToken();

            // Act
            var achievements = await _pathfinderAchievementService.AddAchievementsForPathfinderAsync(pathfinderId, cancellationToken);

            // Assert
            Assert.That(achievements, Is.Not.Null);
            Assert.That(achievements.Count, Is.EqualTo(expectedAchievementsCount));
            foreach (var achievement in achievements)
            {
                Assert.That(achievement.PathfinderID, Is.EqualTo(pathfinderId));
                Assert.That(achievement.Grade, Is.EqualTo(grade));
            }
        }

        [Test]
        public async Task GetAllAchievementsForPathfinderAsync_DefaultBehavior_FiltersByPathfinderGrade()
        {
            // Arrange
            var pathfinder = _pathfinders.First(p => p.Grade.HasValue);
            var pathfinderId = pathfinder.PathfinderID;
            var pathfinderGrade = pathfinder.Grade.Value;
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.GetAllAchievementsForPathfinderAsync(pathfinderId, false, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ICollection<PathfinderAchievementDto>>());
            Assert.That(result.All(a => a.Grade == pathfinderGrade), Is.True, "All achievements should be for the pathfinder's grade");
        }

        [Test]
        public async Task GetAllAchievementsForPathfinderAsync_ShowAllAchievementsTrue_ReturnsAllAchievements()
        {
            // Arrange
            var pathfinder = _pathfinders.First();
            var pathfinderId = pathfinder.PathfinderID;
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.GetAllAchievementsForPathfinderAsync(pathfinderId, true, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ICollection<PathfinderAchievementDto>>());
            Assert.That(result.All(a => a.PathfinderID == pathfinderId), Is.True, "All achievements should be for the specified pathfinder");
        }

        [Test]
        public async Task GetAllAchievementsForPathfinderAsync_PathfinderWithoutGrade_ReturnsEmptyList()
        {
            // Arrange
            var pathfinderWithoutGrade = _pathfinders.FirstOrDefault(p => !p.Grade.HasValue);
            if (pathfinderWithoutGrade == null)
            {
                // Create a pathfinder without grade for testing
                pathfinderWithoutGrade = new Pathfinder
                {
                    PathfinderID = Guid.NewGuid(),
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    Grade = null,
                    ClubID = _pathfinders.First().ClubID
                };
                _dbContext.Pathfinders.Add(pathfinderWithoutGrade);
                await _dbContext.SaveChangesAsync();
            }

            var pathfinderId = pathfinderWithoutGrade.PathfinderID;
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.GetAllAchievementsForPathfinderAsync(pathfinderId, false, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllAsync_ShowAllAchievementsFalse_FiltersByEachPathfinderGrade()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.GetAllAsync(false, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ICollection<PathfinderAchievementDto>>());
            Assert.That(result.Count, Is.GreaterThan(0));

            // Group by pathfinder and verify each pathfinder only has achievements matching their grade
            var pathfinderGroups = result.GroupBy(pa => pa.PathfinderID).ToList();
            
            foreach (var group in pathfinderGroups)
            {
                var pathfinder = _pathfinders.First(p => p.PathfinderID == group.Key);
                
                if (pathfinder.Grade.HasValue)
                {
                    // Pathfinders with a grade should only have achievements matching their grade
                    Assert.That(group.All(pa => pa.Grade == pathfinder.Grade.Value), Is.True, 
                        $"Pathfinder {pathfinder.PathfinderID} (Grade {pathfinder.Grade}) should only have Grade {pathfinder.Grade} achievements");
                }
                else
                {
                    // Pathfinders with null grade should have no achievements
                    Assert.That(group.Count(), Is.EqualTo(0), 
                        $"Pathfinder {pathfinder.PathfinderID} with null grade should have no achievements");
                }
            }
        }

        [Test]
        public async Task GetAllAsync_ShowAllAchievementsTrue_ReturnsAllAchievements()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.GetAllAsync(true, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ICollection<PathfinderAchievementDto>>());
            Assert.That(result.Count, Is.GreaterThan(0));

            // When showAllAchievements is true, pathfinders can have achievements from different grades
            var pathfinderGroups = result.GroupBy(pa => pa.PathfinderID).ToList();
            
            foreach (var group in pathfinderGroups)
            {
                var pathfinder = _pathfinders.First(p => p.PathfinderID == group.Key);
                
                if (pathfinder.Grade.HasValue)
                {
                    // Pathfinders with a grade can have achievements from different grades when showAllAchievements is true
                    var uniqueGrades = group.Select(pa => pa.Grade).Distinct().ToList();
                    Assert.That(uniqueGrades.Count, Is.GreaterThanOrEqualTo(1), 
                        $"Pathfinder {pathfinder.PathfinderID} should have at least one achievement");
                    
                    // The pathfinder should have at least some achievements (the test data may not have achievements for every grade)
                    Assert.That(group.Count(), Is.GreaterThan(0), 
                        $"Pathfinder {pathfinder.PathfinderID} should have at least one achievement");
                }
            }
        }

        [Test]
        public async Task GetAllAsync_ShowAllAchievementsFalse_PathfindersWithNullGradeHaveNoAchievements()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.GetAllAsync(false, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);

            // Get pathfinders with null grades
            var pathfindersWithNullGrade = _pathfinders.Where(p => p.Grade == null).ToList();
            
            foreach (var pathfinder in pathfindersWithNullGrade)
            {
                var pathfinderAchievements = result.Where(pa => pa.PathfinderID == pathfinder.PathfinderID).ToList();
                Assert.That(pathfinderAchievements.Count, Is.EqualTo(0), 
                    $"Pathfinder {pathfinder.PathfinderID} with null grade should have no achievements");
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            await DatabaseCleaner.CleanDatabase(_dbContext);
            _dbContext.Dispose();
        }
    }
}
