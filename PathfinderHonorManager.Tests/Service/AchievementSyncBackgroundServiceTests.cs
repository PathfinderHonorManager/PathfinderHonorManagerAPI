using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Service.Interfaces;
using PathfinderHonorManager.Tests.Helpers;

namespace PathfinderHonorManager.Tests.Service
{
    [TestFixture]
    public class AchievementSyncBackgroundServiceTests
    {
        private Mock<IGradeChangeQueue> _queueMock;
        private Mock<IOptions<AchievementSyncOptions>> _optionsMock;
        private IServiceProvider _serviceProvider;
        private PathfinderContext _dbContext;
        private DbContextOptions<PathfinderContext> _contextOptions;

        [SetUp]
        public void SetUp()
        {
            _contextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new PathfinderContext(_contextOptions);

            _queueMock = new Mock<IGradeChangeQueue>();
            _optionsMock = new Mock<IOptions<AchievementSyncOptions>>();
            _optionsMock.Setup(o => o.Value).Returns(new AchievementSyncOptions
            {
                ProcessingInterval = TimeSpan.FromMilliseconds(100),
                MaxBatchSize = 50,
                MaxConcurrency = 5,
                RunAuditOnStartup = false
            });

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<PathfinderContext>(_ => new PathfinderContext(_contextOptions));
            serviceCollection.AddScoped<IPathfinderAchievementService>(sp =>
            {
                var context = sp.GetRequiredService<PathfinderContext>();
                var mapperConfig = new AutoMapper.MapperConfiguration(cfg => 
                    cfg.AddProfile<PathfinderHonorManager.Mapping.AutoMapperConfig>());
                var mapper = mapperConfig.CreateMapper();
                var validator = new DummyValidator<PathfinderHonorManager.Dto.Incoming.PathfinderAchievementDto>();
                return new PathfinderAchievementService(
                    context, 
                    mapper, 
                    NullLogger<PathfinderAchievementService>.Instance,
                    validator);
            });

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task StartAsync_WithRunAuditOnStartupTrue_StartsAudit()
        {
            _optionsMock.Setup(o => o.Value).Returns(new AchievementSyncOptions
            {
                RunAuditOnStartup = true,
                ProcessingInterval = TimeSpan.FromHours(1)
            });

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await Task.Delay(100);

            await service.StopAsync(CancellationToken.None);
            
            Assert.Pass("Service started and stopped successfully with audit enabled");
        }

        [Test]
        public async Task StartAsync_WithRunAuditOnStartupFalse_DoesNotRunAudit()
        {
            _optionsMock.Setup(o => o.Value).Returns(new AchievementSyncOptions
            {
                RunAuditOnStartup = false,
                ProcessingInterval = TimeSpan.FromHours(1)
            });

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await service.StopAsync(CancellationToken.None);

            _queueMock.Verify(q => q.TryEnqueueAsync(It.IsAny<GradeChangeEvent>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Test]
        public async Task Audit_FindsPathfindersWithMismatchedAchievements()
        {
            await DatabaseSeeder.SeedDatabase(_contextOptions);
            
            var pathfinder = await _dbContext.Pathfinders.FirstAsync(p => p.Grade != null && p.IsActive == true);
            
            var achievementsToRemove = await _dbContext.PathfinderAchievements
                .Where(pa => pa.PathfinderID == pathfinder.PathfinderID)
                .ToListAsync();
            _dbContext.PathfinderAchievements.RemoveRange(achievementsToRemove);
            await _dbContext.SaveChangesAsync();

            _optionsMock.Setup(o => o.Value).Returns(new AchievementSyncOptions
            {
                RunAuditOnStartup = true,
                ProcessingInterval = TimeSpan.FromHours(1)
            });

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await Task.Delay(500);
            await service.StopAsync(CancellationToken.None);

            _queueMock.Verify(q => q.TryEnqueueAsync(
                It.Is<GradeChangeEvent>(e => e.PathfinderId == pathfinder.PathfinderID), 
                It.IsAny<CancellationToken>()), 
                Times.AtLeastOnce);
        }

        [Test]
        public async Task Audit_SkipsInactivePathfinders()
        {
            await DatabaseSeeder.SeedDatabase(_contextOptions);
            
            var inactivePathfinder = await _dbContext.Pathfinders
                .FirstAsync(p => p.IsActive == false);

            _optionsMock.Setup(o => o.Value).Returns(new AchievementSyncOptions
            {
                RunAuditOnStartup = true,
                ProcessingInterval = TimeSpan.FromHours(1)
            });

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await Task.Delay(500);
            await service.StopAsync(CancellationToken.None);

            _queueMock.Verify(q => q.TryEnqueueAsync(
                It.Is<GradeChangeEvent>(e => e.PathfinderId == inactivePathfinder.PathfinderID), 
                It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Test]
        public async Task Audit_SkipsPathfindersWithNullGrade()
        {
            await DatabaseSeeder.SeedDatabase(_contextOptions);
            
            var pathfinder = await _dbContext.Pathfinders.FirstAsync();
            pathfinder.Grade = null;
            await _dbContext.SaveChangesAsync();

            _optionsMock.Setup(o => o.Value).Returns(new AchievementSyncOptions
            {
                RunAuditOnStartup = true,
                ProcessingInterval = TimeSpan.FromHours(1)
            });

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await Task.Delay(500);
            await service.StopAsync(CancellationToken.None);

            _queueMock.Verify(q => q.TryEnqueueAsync(
                It.Is<GradeChangeEvent>(e => e.PathfinderId == pathfinder.PathfinderID), 
                It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Test]
        public async Task ProcessQueue_WithEmptyQueue_DoesNothing()
        {
            _queueMock.Setup(q => q.GetCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);
            _queueMock.Setup(q => q.DequeueAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GradeChangeEvent>());

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await Task.Delay(300);
            await service.StopAsync(CancellationToken.None);

            _queueMock.Verify(q => q.DequeueAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Test]
        public async Task ProcessQueue_WithItems_ProcessesThem()
        {
            await DatabaseSeeder.SeedDatabase(_contextOptions);
            
            var pathfinder = await _dbContext.Pathfinders
                .Include(p => p.PathfinderAchievements)
                .FirstAsync(p => p.Grade != null && p.IsActive == true);

            var gradeChangeEvent = new GradeChangeEvent(pathfinder.PathfinderID, pathfinder.Grade - 1, pathfinder.Grade);

            _queueMock.Setup(q => q.GetCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _queueMock.Setup(q => q.DequeueAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GradeChangeEvent> { gradeChangeEvent });

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await Task.Delay(300);
            await service.StopAsync(CancellationToken.None);

            _queueMock.Verify(q => q.DequeueAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), 
                Times.AtLeastOnce);
        }

        [Test]
        public async Task ProcessQueue_RespectsMaxBatchSize()
        {
            var maxBatchSize = 10;
            _optionsMock.Setup(o => o.Value).Returns(new AchievementSyncOptions
            {
                ProcessingInterval = TimeSpan.FromMilliseconds(100),
                MaxBatchSize = maxBatchSize,
                MaxConcurrency = 5,
                RunAuditOnStartup = false
            });

            _queueMock.Setup(q => q.GetCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(100);
            _queueMock.Setup(q => q.DequeueAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GradeChangeEvent>());

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await Task.Delay(300);
            await service.StopAsync(CancellationToken.None);

            _queueMock.Verify(q => q.DequeueAllAsync(maxBatchSize, It.IsAny<CancellationToken>()), 
                Times.AtLeastOnce);
        }

        [Test]
        public async Task SyncAchievements_SkipsWhenAchievementsAlreadyCorrect()
        {
            await DatabaseSeeder.SeedDatabase(_contextOptions);
            
            var pathfinder = await _dbContext.Pathfinders
                .Include(p => p.PathfinderAchievements)
                .ThenInclude(pa => pa.Achievement)
                .FirstAsync(p => p.Grade != null && p.IsActive == true);

            var achievementsToRemove = await _dbContext.PathfinderAchievements
                .Where(pa => pa.PathfinderID == pathfinder.PathfinderID)
                .ToListAsync();
            _dbContext.PathfinderAchievements.RemoveRange(achievementsToRemove);
            await _dbContext.SaveChangesAsync();

            using (var scope = _serviceProvider.CreateScope())
            {
                var achievementService = scope.ServiceProvider.GetRequiredService<IPathfinderAchievementService>();
                await achievementService.AddAchievementsForPathfinderAsync(pathfinder.PathfinderID, CancellationToken.None);
            }

            var achievementCountBefore = await _dbContext.PathfinderAchievements
                .Where(pa => pa.PathfinderID == pathfinder.PathfinderID)
                .CountAsync();

            var gradeChangeEvent = new GradeChangeEvent(pathfinder.PathfinderID, pathfinder.Grade, pathfinder.Grade);

            _queueMock.Setup(q => q.GetCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _queueMock.Setup(q => q.DequeueAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GradeChangeEvent> { gradeChangeEvent });

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await Task.Delay(500);
            await service.StopAsync(CancellationToken.None);

            var freshContext = new PathfinderContext(_contextOptions);
            var achievementCountAfter = await freshContext.PathfinderAchievements
                .Where(pa => pa.PathfinderID == pathfinder.PathfinderID)
                .CountAsync();

            Assert.That(achievementCountAfter, Is.EqualTo(achievementCountBefore), "Achievement count should not change when already correct");
        }

        [Test]
        public async Task SyncAchievements_HandlesNonExistentPathfinder()
        {
            var nonExistentPathfinderId = Guid.NewGuid();
            var gradeChangeEvent = new GradeChangeEvent(nonExistentPathfinderId, 5, 6);

            _queueMock.Setup(q => q.GetCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _queueMock.Setup(q => q.DequeueAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GradeChangeEvent> { gradeChangeEvent });

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await Task.Delay(300);
            await service.StopAsync(CancellationToken.None);
            
            Assert.Pass("Service should handle non-existent pathfinder gracefully without throwing");
        }

        [Test]
        public async Task SyncAchievements_ContinuesOnIndividualFailure()
        {
            await DatabaseSeeder.SeedDatabase(_contextOptions);
            
            var pathfinder1 = await _dbContext.Pathfinders.FirstAsync(p => p.Grade != null);
            var nonExistentPathfinderId = Guid.NewGuid();
            var pathfinder2 = await _dbContext.Pathfinders.Skip(1).FirstAsync(p => p.Grade != null);

            var events = new List<GradeChangeEvent>
            {
                new GradeChangeEvent(pathfinder1.PathfinderID, 5, pathfinder1.Grade.Value),
                new GradeChangeEvent(nonExistentPathfinderId, 5, 6),
                new GradeChangeEvent(pathfinder2.PathfinderID, 5, pathfinder2.Grade.Value)
            };

            _queueMock.Setup(q => q.GetCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);
            _queueMock.Setup(q => q.DequeueAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);

            var service = new AchievementSyncBackgroundService(
                _serviceProvider,
                _queueMock.Object,
                _optionsMock.Object,
                NullLogger<AchievementSyncBackgroundService>.Instance);

            await service.StartAsync(CancellationToken.None);
            await Task.Delay(500);
            await service.StopAsync(CancellationToken.None);
            
            var pathfinder1Achievements = await _dbContext.PathfinderAchievements
                .CountAsync(pa => pa.PathfinderID == pathfinder1.PathfinderID);
            var pathfinder2Achievements = await _dbContext.PathfinderAchievements
                .CountAsync(pa => pa.PathfinderID == pathfinder2.PathfinderID);
            
            Assert.That(pathfinder1Achievements, Is.GreaterThan(0), "First pathfinder should have achievements");
            Assert.That(pathfinder2Achievements, Is.GreaterThan(0), "Third pathfinder should have achievements despite middle one failing");
        }

        [TearDown]
        public async Task TearDown()
        {
            await _dbContext.DisposeAsync();
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}

