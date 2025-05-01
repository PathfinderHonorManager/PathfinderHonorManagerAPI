using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Service.Interfaces;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

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

        public async Task<Outgoing.ClubDto> CreateAsync(Incoming.ClubDto club, CancellationToken token)
        {
            _logger.LogInformation("Creating new club with code: {ClubCode}", club.ClubCode);

            try
            {
                var entity = _mapper.Map<Club>(club);
                await _dbContext.Clubs.AddAsync(entity, token);
                await _dbContext.SaveChangesAsync(token);

                _logger.LogInformation("Created club: {ClubName} (ID: {ClubId})", entity.Name, entity.ClubID);
                return _mapper.Map<Outgoing.ClubDto>(entity);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating club with code {ClubCode}", club.ClubCode);
                throw new ValidationException("Failed to create club. The club code may already be in use.");
            }
        }

        public async Task<Outgoing.ClubDto> UpdateAsync(Guid id, Incoming.ClubDto club, CancellationToken token)
        {
            _logger.LogInformation("Updating club with ID: {ClubId}", id);

            try
            {
                var entity = await _dbContext.Clubs
                    .SingleOrDefaultAsync(c => c.ClubID == id, token);

                if (entity == default)
                {
                    _logger.LogWarning("Club with ID {ClubId} not found", id);
                    return default;
                }

                _mapper.Map(club, entity);
                await _dbContext.SaveChangesAsync(token);

                _logger.LogInformation("Updated club: {ClubName} (ID: {ClubId})", entity.Name, id);
                return _mapper.Map<Outgoing.ClubDto>(entity);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating club with ID {ClubId}", id);
                throw new ValidationException("Failed to update club. The club code may already be in use.");
            }
        }
    }
}
