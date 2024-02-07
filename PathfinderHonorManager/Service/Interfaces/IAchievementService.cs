using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Service.Interfaces
{
    public interface IAchievementService
    {
        Task<ICollection<Outgoing.AchievementDto>> GetAllAsync(CancellationToken token);
        Task<Outgoing.AchievementDto> GetByIdAsync(Guid id, CancellationToken token);
    }
}