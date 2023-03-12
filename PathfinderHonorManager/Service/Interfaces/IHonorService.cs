using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Service.Interfaces
{
    public interface IHonorService
    {
        Task<Outgoing.HonorDto> GetByIdAsync(
            Guid id,
            CancellationToken token);

        Task<ICollection<Outgoing.HonorDto>> GetAllAsync(
            CancellationToken token);

        Task<Outgoing.HonorDto> AddAsync(
            Incoming.HonorDto newHonor,
            CancellationToken token);

        Task<Outgoing.HonorDto> UpdateAsync(
            Guid id,
            Incoming.HonorDto updatedHonor,
            CancellationToken token);

    }
}
