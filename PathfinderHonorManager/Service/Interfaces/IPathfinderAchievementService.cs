using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Service.Interfaces
{
    public interface IPathfinderAchievementService
    {
        Task<ICollection<Outgoing.PathfinderAchievementDto>> GetAllAsync(CancellationToken token);
        Task<Outgoing.PathfinderAchievementDto> GetByIdAsync(Guid id, CancellationToken token);
    }
}