using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;

namespace PathfinderHonorManager.Service
{
    public class AchievementSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IGradeChangeQueue _gradeChangeQueue;
        private readonly ILogger<AchievementSyncBackgroundService> _logger;
        private readonly AchievementSyncOptions _options;

        public AchievementSyncBackgroundService(
            IServiceProvider serviceProvider,
            IGradeChangeQueue gradeChangeQueue,
            IOptions<AchievementSyncOptions> options,
            ILogger<AchievementSyncBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _gradeChangeQueue = gradeChangeQueue;
            _options = options.Value;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Achievement sync background service starting");

            if (_options.RunAuditOnStartup)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await AuditAndQueueDiscrepanciesAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during startup audit");
                    }
                }, cancellationToken);
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Achievement sync background service running with interval: {Interval}",
                _options.ProcessingInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.ProcessingInterval, stoppingToken);
                    await ProcessQueueAsync(stoppingToken);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation(ex, "Achievement sync background service stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in achievement sync processing loop");
                }
            }
        }

        private async Task AuditAndQueueDiscrepanciesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running startup audit to find mismatched achievements");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PathfinderContext>();

                var pathfindersWithAchievements = await dbContext.Pathfinders
                    .Where(p => p.Grade != null && p.IsActive == true)
                    .Include(p => p.PathfinderAchievements)
                        .ThenInclude(pa => pa.Achievement)
                    .Select(p => new
                    {
                        p.PathfinderID,
                        CurrentGrade = p.Grade.Value,
                        AchievementGrades = p.PathfinderAchievements
                            .Select(pa => pa.Achievement.Grade)
                            .Distinct()
                            .ToList()
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Auditing {Count} active pathfinders with grades", pathfindersWithAchievements.Count);

                int queuedCount = 0;

                foreach (var pathfinder in pathfindersWithAchievements)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    bool needsSync = pathfinder.AchievementGrades.Count == 0 || 
                                    !pathfinder.AchievementGrades.Contains(pathfinder.CurrentGrade);

                    if (needsSync)
                    {
                        var gradeChange = new GradeChangeEvent(
                            pathfinder.PathfinderID,
                            pathfinder.AchievementGrades.FirstOrDefault(),
                            pathfinder.CurrentGrade);

                        var enqueued = await _gradeChangeQueue.TryEnqueueAsync(gradeChange, cancellationToken);
                        if (enqueued)
                        {
                            queuedCount++;
                        }
                    }
                }

                _logger.LogInformation(
                    "Startup audit complete: {QueuedCount} pathfinders queued for achievement sync",
                    queuedCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to complete startup audit of pathfinder achievements", ex);
            }
        }


        private async Task ProcessQueueAsync(CancellationToken cancellationToken)
        {
            var queueCount = await _gradeChangeQueue.GetCountAsync(cancellationToken);
            
            if (queueCount == 0)
            {
                return;
            }

            _logger.LogInformation("Processing achievement sync queue: {Count} items", queueCount);

            var stopwatch = Stopwatch.StartNew();
            int successCount = 0;
            int failedCount = 0;

            var items = await _gradeChangeQueue.DequeueAllAsync(_options.MaxBatchSize, cancellationToken);
            var itemsList = items.ToList();

            var semaphore = new SemaphoreSlim(_options.MaxConcurrency);
            var tasks = new List<Task>();

            foreach (var item in itemsList)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        await SyncAchievementsForPathfinderAsync(item, cancellationToken);
                        Interlocked.Increment(ref successCount);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failedCount);
                        _logger.LogError(
                            ex,
                            "Failed to sync achievements for pathfinder {PathfinderId}. Will retry on next startup audit.",
                            item.PathfinderId);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            _logger.LogInformation(
                "Processing cycle complete: {SuccessCount} succeeded, {FailedCount} failed, Duration: {Duration}ms",
                successCount,
                failedCount,
                stopwatch.ElapsedMilliseconds);
        }

        private async Task SyncAchievementsForPathfinderAsync(
            GradeChangeEvent gradeChange,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PathfinderContext>();

            _logger.LogInformation(
                "Syncing achievements for pathfinder {PathfinderId}: Grade {OldGrade} → {NewGrade}",
                gradeChange.PathfinderId,
                gradeChange.OldGrade?.ToString() ?? "null",
                gradeChange.NewGrade?.ToString() ?? "null");

            var pathfinder = await dbContext.Pathfinders
                .FirstOrDefaultAsync(p => p.PathfinderID == gradeChange.PathfinderId, cancellationToken);

            if (pathfinder == null)
            {
                _logger.LogWarning(
                    "Pathfinder {PathfinderId} not found, skipping achievement sync",
                    gradeChange.PathfinderId);
                return;
            }

            if (pathfinder.Grade == null)
            {
                _logger.LogWarning(
                    "Pathfinder {PathfinderId} has no grade, skipping achievement sync",
                    gradeChange.PathfinderId);
                return;
            }

            var existingAchievementIds = await dbContext.PathfinderAchievements
                .Where(pa => pa.PathfinderID == gradeChange.PathfinderId)
                .Select(pa => pa.AchievementID)
                .ToListAsync(cancellationToken);

            var achievementsForGrade = await dbContext.Achievements
                .Where(a => a.Grade == pathfinder.Grade)
                .Select(a => a.AchievementID)
                .ToListAsync(cancellationToken);

            var missingAchievementIds = achievementsForGrade
                .Except(existingAchievementIds)
                .ToList();

            if (missingAchievementIds.Count == 0)
            {
                _logger.LogDebug(
                    "Pathfinder {PathfinderId} already has all achievements for grade {Grade}, skipping sync",
                    gradeChange.PathfinderId,
                    pathfinder.Grade);
                return;
            }

            _logger.LogInformation(
                "Adding {Count} missing achievements for pathfinder {PathfinderId} (grade {Grade})",
                missingAchievementIds.Count,
                gradeChange.PathfinderId,
                pathfinder.Grade);

            var strategy = dbContext.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                
                try
                {
                    var newPathfinderAchievements = missingAchievementIds.Select(achievementId => 
                        new PathfinderAchievement
                        {
                            PathfinderID = gradeChange.PathfinderId,
                            AchievementID = achievementId,
                            IsAchieved = false,
                            AchieveTimestamp = null
                        }).ToList();

                    await dbContext.PathfinderAchievements.AddRangeAsync(newPathfinderAchievements, cancellationToken);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Successfully added {Count} achievements for pathfinder {PathfinderId}",
                        missingAchievementIds.Count,
                        gradeChange.PathfinderId);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Achievement sync background service stopping gracefully");
            await base.StopAsync(cancellationToken);
        }
    }
}

