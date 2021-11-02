using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Service.Interfaces
{
    public interface IHonorService
    {
        Task<Outgoing.HonorDto> GetByIdAsync(
            Guid id,
            CancellationToken token);

        Task<ICollection<Outgoing.HonorDto>> GetAllAsync(CancellationToken token);
    }
}
