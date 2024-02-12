using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Service.Interfaces
{
    public interface IPathfinderAchievementService
    {
        Task<ICollection<Outgoing.PathfinderAchievementDto>> GetAllAsync(CancellationToken token);
        Task<Outgoing.PathfinderAchievementDto> GetByIdAsync(Guid pathfinderId, Guid achievementId, CancellationToken token);
        Task<Outgoing.PathfinderAchievementDto> AddAsync(Guid pathfinderId, Incoming.PostPathfinderAchievementDto achievementId, CancellationToken token);
    }
}