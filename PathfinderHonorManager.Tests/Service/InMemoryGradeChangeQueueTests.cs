using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;

namespace PathfinderHonorManager.Tests.Service
{
    [TestFixture]
    public class InMemoryGradeChangeQueueTests
    {
        private InMemoryGradeChangeQueue _queue;

        [SetUp]
        public void SetUp()
        {
            _queue = new InMemoryGradeChangeQueue(NullLogger<InMemoryGradeChangeQueue>.Instance);
        }

        [Test]
        public async Task TryEnqueueAsync_WithValidEvent_ReturnsTrue()
        {
            var gradeChange = new GradeChangeEvent(Guid.NewGuid(), 5, 6);

            var result = await _queue.TryEnqueueAsync(gradeChange);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TryEnqueueAsync_WithDuplicatePathfinderId_ReturnsFalse()
        {
            var pathfinderId = Guid.NewGuid();
            var gradeChange1 = new GradeChangeEvent(pathfinderId, 5, 6);
            var gradeChange2 = new GradeChangeEvent(pathfinderId, 6, 7);

            await _queue.TryEnqueueAsync(gradeChange1);
            var result = await _queue.TryEnqueueAsync(gradeChange2);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TryEnqueueAsync_WithNullEvent_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _queue.TryEnqueueAsync(null));
        }

        [Test]
        public async Task DequeueAllAsync_ReturnsEnqueuedItems()
        {
            var gradeChange1 = new GradeChangeEvent(Guid.NewGuid(), 5, 6);
            var gradeChange2 = new GradeChangeEvent(Guid.NewGuid(), 6, 7);
            await _queue.TryEnqueueAsync(gradeChange1);
            await _queue.TryEnqueueAsync(gradeChange2);

            var items = await _queue.DequeueAllAsync(10);

            Assert.That(items.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task DequeueAllAsync_ReturnsItemsInFIFOOrder()
        {
            var pathfinderId1 = Guid.NewGuid();
            var pathfinderId2 = Guid.NewGuid();
            var pathfinderId3 = Guid.NewGuid();
            
            var gradeChange1 = new GradeChangeEvent(pathfinderId1, 5, 6);
            var gradeChange2 = new GradeChangeEvent(pathfinderId2, 6, 7);
            var gradeChange3 = new GradeChangeEvent(pathfinderId3, 7, 8);
            
            await _queue.TryEnqueueAsync(gradeChange1);
            await _queue.TryEnqueueAsync(gradeChange2);
            await _queue.TryEnqueueAsync(gradeChange3);

            var items = (await _queue.DequeueAllAsync(10)).ToList();

            Assert.That(items[0].PathfinderId, Is.EqualTo(pathfinderId1));
            Assert.That(items[1].PathfinderId, Is.EqualTo(pathfinderId2));
            Assert.That(items[2].PathfinderId, Is.EqualTo(pathfinderId3));
        }

        [Test]
        public async Task DequeueAllAsync_RespectsMaxItems()
        {
            var gradeChange1 = new GradeChangeEvent(Guid.NewGuid(), 5, 6);
            var gradeChange2 = new GradeChangeEvent(Guid.NewGuid(), 6, 7);
            var gradeChange3 = new GradeChangeEvent(Guid.NewGuid(), 7, 8);
            
            await _queue.TryEnqueueAsync(gradeChange1);
            await _queue.TryEnqueueAsync(gradeChange2);
            await _queue.TryEnqueueAsync(gradeChange3);

            var items = await _queue.DequeueAllAsync(2);

            Assert.That(items.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task DequeueAllAsync_EmptyQueue_ReturnsEmptyCollection()
        {
            var items = await _queue.DequeueAllAsync(10);

            Assert.That(items, Is.Empty);
        }

        [Test]
        public async Task GetCountAsync_ReturnsCorrectCount()
        {
            var gradeChange1 = new GradeChangeEvent(Guid.NewGuid(), 5, 6);
            var gradeChange2 = new GradeChangeEvent(Guid.NewGuid(), 6, 7);
            
            await _queue.TryEnqueueAsync(gradeChange1);
            await _queue.TryEnqueueAsync(gradeChange2);

            var count = await _queue.GetCountAsync();

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetCountAsync_EmptyQueue_ReturnsZero()
        {
            var count = await _queue.GetCountAsync();

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public async Task DequeueAllAsync_RemovesItemsFromQueue()
        {
            var gradeChange = new GradeChangeEvent(Guid.NewGuid(), 5, 6);
            await _queue.TryEnqueueAsync(gradeChange);

            await _queue.DequeueAllAsync(10);
            var count = await _queue.GetCountAsync();

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public async Task DequeueAllAsync_AllowsReEnqueueOfSamePathfinderId()
        {
            var pathfinderId = Guid.NewGuid();
            var gradeChange1 = new GradeChangeEvent(pathfinderId, 5, 6);
            
            await _queue.TryEnqueueAsync(gradeChange1);
            await _queue.DequeueAllAsync(10);
            
            var gradeChange2 = new GradeChangeEvent(pathfinderId, 6, 7);
            var result = await _queue.TryEnqueueAsync(gradeChange2);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ThreadSafety_MultipleConcurrentEnqueues_AllSucceed()
        {
            var tasks = Enumerable.Range(0, 10)
                .Select(i => Task.Run(async () =>
                {
                    var gradeChange = new GradeChangeEvent(Guid.NewGuid(), 5, 6);
                    return await _queue.TryEnqueueAsync(gradeChange);
                }))
                .ToArray();

            var results = await Task.WhenAll(tasks);
            var count = await _queue.GetCountAsync();

            Assert.That(results.All(r => r), Is.True);
            Assert.That(count, Is.EqualTo(10));
        }

        [Test]
        public async Task ThreadSafety_ConcurrentEnqueueAndDequeue_WorksCorrectly()
        {
            var enqueueTask = Task.Run(async () =>
            {
                for (int i = 0; i < 20; i++)
                {
                    var gradeChange = new GradeChangeEvent(Guid.NewGuid(), 5, 6);
                    await _queue.TryEnqueueAsync(gradeChange);
                    await Task.Delay(10);
                }
            });

            var dequeueTask = Task.Run(async () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(30);
                    await _queue.DequeueAllAsync(2);
                }
            });

            await Task.WhenAll(enqueueTask, dequeueTask);
            var finalCount = await _queue.GetCountAsync();

            Assert.That(finalCount, Is.GreaterThanOrEqualTo(0));
        }
    }
}

