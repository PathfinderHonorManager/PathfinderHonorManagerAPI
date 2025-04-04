using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Service.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace PathfinderHonorManager.Service
{
    public class ClubService : IClubService
    {
        private readonly PathfinderContext _dbContext;

        private readonly IMapper _mapper;

        private readonly ILogger _logger;

        public ClubService(
            PathfinderContext context,
            IMapper mapper,
            ILogger<ClubService> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ICollection<Outgoing.ClubDto>> GetAllAsync(CancellationToken token)
        {
            _logger.LogInformation("Retrieving all clubs");
            var clubs = await _dbContext.Clubs
                .OrderBy(h => h.Name)
                .ToListAsync(token);

            _logger.LogInformation("Retrieved {Count} clubs", clubs.Count);
            return _mapper.Map<ICollection<Outgoing.ClubDto>>(clubs);
        }

        public async Task<Outgoing.ClubDto> GetByIdAsync(Guid id, CancellationToken token)
        {
            _logger.LogInformation("Retrieving club with ID: {ClubId}", id);
            Club entity;

            entity = await _dbContext.Clubs
                .SingleOrDefaultAsync(p => p.ClubID == id, token);

            if (entity == default)
            {
                _logger.LogWarning("Club with ID {ClubId} not found", id);
                return default;
            }

            _logger.LogInformation("Retrieved club: {ClubName} (ID: {ClubId})", entity.Name, id);
            return _mapper.Map<Outgoing.ClubDto>(entity);
        }

        public async Task<Outgoing.ClubDto> GetByCodeAsync(string code, CancellationToken token)
        {
            code = code.ToUpper();
            _logger.LogInformation("Retrieving club with code: {ClubCode}", code);

            Club entity;

            entity = await _dbContext.Clubs
                .SingleOrDefaultAsync(p => p.ClubCode == code, token);

            if (entity == default)
            {
                _logger.LogWarning("Club with code {ClubCode} not found", code);
                return default;
            }

            _logger.LogInformation("Retrieved club: {ClubName} (Code: {ClubCode})", entity.Name, code);
            return _mapper.Map<Outgoing.ClubDto>(entity);
        }
    }
}
