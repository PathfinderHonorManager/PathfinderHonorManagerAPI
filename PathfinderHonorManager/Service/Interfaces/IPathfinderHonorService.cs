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
        Task<Outgoing.PathfinderHonorDto> GetByIdAsync(
        Guid id,
        CancellationToken token);

        Task<Outgoing.PathfinderHonorDto> AddAsync(
            Incoming.PathfinderHonorDto newPathfinder,
            CancellationToken token);
    }

}
