using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Service.Interfaces
{

    public interface IPathfinderHonorService
    {
        Task<ICollection<Outgoing.PathfinderHonorDto>> GetAllAsync(Guid pathfinderId, CancellationToken token);

        Task<Outgoing.PathfinderHonorDto> GetByIdAsync(
        Guid pathfinderId,
        Guid pathfinderHonorId,
        CancellationToken token);

        Task<Outgoing.PathfinderHonorDto> AddAsync(
            Guid pathfinderId,
            Incoming.PostPathfinderHonorDto newPathfinder,
            CancellationToken token);

        Task<Outgoing.PathfinderHonorDto> UpdateAsync(
            Guid pathfinderId,
            Guid honorId,
            Incoming.PutPathfinderHonorDto incomingPathfinderHonor,
            CancellationToken token);
    }

}
