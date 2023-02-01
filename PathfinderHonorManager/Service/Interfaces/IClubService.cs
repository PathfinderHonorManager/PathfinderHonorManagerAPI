using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Service.Interfaces
{
    public interface IClubService
    {
        Task<Outgoing.ClubDto> GetByIdAsync(
            Guid id,
            CancellationToken token);

        Task<Outgoing.ClubDto> GetByCodeAsync(
            string code,
            CancellationToken token);

        Task<ICollection<Outgoing.ClubDto>> GetAllAsync(
            CancellationToken token);
    }
}
