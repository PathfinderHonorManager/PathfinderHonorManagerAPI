using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Incoming = PathfinderHonorManager.Dto.Incoming;

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

        Task<Outgoing.ClubDto> CreateAsync(
            Incoming.ClubDto club,
            CancellationToken token);

        Task<Outgoing.ClubDto> UpdateAsync(
            Guid id,
            Incoming.ClubDto club,
            CancellationToken token);
    }
}
