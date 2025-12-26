using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PathfinderHonorManager.Model;

namespace PathfinderHonorManager.Service.Interfaces
{
    public interface IGradeChangeQueue
    {
        Task<bool> TryEnqueueAsync(GradeChangeEvent gradeChange, CancellationToken token = default);
        
        Task<IEnumerable<GradeChangeEvent>> DequeueAllAsync(int maxItems, CancellationToken token = default);
        
        Task<int> GetCountAsync(CancellationToken token = default);
    }
}

