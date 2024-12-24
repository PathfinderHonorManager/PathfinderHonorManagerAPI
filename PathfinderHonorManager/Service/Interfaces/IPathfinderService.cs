using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Service.Interfaces
{

    public interface IPathfinderService
    {
        Task<Outgoing.PathfinderDependantDto> GetByIdAsync(
            Guid id,
            string clubCode,
            CancellationToken token);

        Task<ICollection<Outgoing.PathfinderDependantDto>> GetAllAsync(
            string clubCode,
            bool showInactive,
            CancellationToken token);

        Task<Outgoing.PathfinderDto> AddAsync(
            Incoming.PathfinderDto newPathfinder,
            string clubCode,
            CancellationToken token);

        Task<Outgoing.PathfinderDto> UpdateAsync(
            Guid pathfinderId,
            Incoming.PutPathfinderDto updatedPathfinder,
            string clubCode,
            CancellationToken token);

        Task<ICollection<Outgoing.PathfinderDto>> BulkUpdateAsync(
            IEnumerable<Incoming.BulkPutPathfinderDto> bulkData,
            string clubCode,
            CancellationToken token);
    }

}