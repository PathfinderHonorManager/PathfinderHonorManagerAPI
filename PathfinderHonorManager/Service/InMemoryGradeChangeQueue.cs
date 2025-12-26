using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;

namespace PathfinderHonorManager.Service
{
    public class InMemoryGradeChangeQueue : IGradeChangeQueue
    {
        private readonly Queue<GradeChangeEvent> _queue = new();
        private readonly HashSet<Guid> _queuedPathfinders = new();
        private readonly object _lock = new();
        private readonly ILogger<InMemoryGradeChangeQueue> _logger;

        public InMemoryGradeChangeQueue(ILogger<InMemoryGradeChangeQueue> logger)
        {
            _logger = logger;
        }

        public Task<bool> TryEnqueueAsync(GradeChangeEvent gradeChange, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(gradeChange);

            lock (_lock)
            {
                if (_queuedPathfinders.Contains(gradeChange.PathfinderId))
                {
                    _logger.LogDebug(
                        "Pathfinder {PathfinderId} already queued for achievement sync, skipping duplicate",
                        gradeChange.PathfinderId);
                    return Task.FromResult(false);
                }

                _queue.Enqueue(gradeChange);
                _queuedPathfinders.Add(gradeChange.PathfinderId);
                
                _logger.LogInformation(
                    "Grade change detected for pathfinder {PathfinderId}: {OldGrade} → {NewGrade}, queued for achievement sync",
                    gradeChange.PathfinderId,
                    gradeChange.OldGrade?.ToString() ?? "null",
                    gradeChange.NewGrade?.ToString() ?? "null");
                
                return Task.FromResult(true);
            }
        }

        public Task<IEnumerable<GradeChangeEvent>> DequeueAllAsync(int maxItems, CancellationToken token = default)
        {
            var items = new List<GradeChangeEvent>();

            lock (_lock)
            {
                var itemsToDequeue = Math.Min(maxItems, _queue.Count);
                
                for (int i = 0; i < itemsToDequeue; i++)
                {
                    var item = _queue.Dequeue();
                    items.Add(item);
                    _queuedPathfinders.Remove(item.PathfinderId);
                }
            }

            return Task.FromResult<IEnumerable<GradeChangeEvent>>(items);
        }

        public Task<int> GetCountAsync(CancellationToken token = default)
        {
            lock (_lock)
            {
                return Task.FromResult(_queue.Count);
            }
        }
    }
}

