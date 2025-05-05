using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Model.Enum;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Service.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace PathfinderHonorManager.Service
{
    public class PathfinderHonorService : IPathfinderHonorService
    {
        private readonly PathfinderContext _dbContext;

        private readonly IMapper _mapper;

        private readonly ILogger<PathfinderHonorService> _logger;

        private readonly IValidator<Incoming.PathfinderHonorDto> _validator;

        private PathfinderHonorStatus _honorStatus;

        public PathfinderHonorService(
            PathfinderContext context,
            IMapper mapper,
            IValidator<Incoming.PathfinderHonorDto> validator,
            ILogger<PathfinderHonorService> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<ICollection<Outgoing.PathfinderHonorDto>> GetAllAsync(Guid pathfinderId, CancellationToken token)
        {
            _logger.LogInformation("Getting all honors for pathfinder with ID {PathfinderId}", pathfinderId);
            var pathfinderhonors = await _dbContext.PathfinderHonors
                .Include(phs => phs.PathfinderHonorStatus)
                .Include(h => h.Honor)
                .Where(p => p.PathfinderID == pathfinderId)
                .OrderBy(ph => ph.Honor.Name)
                .ToListAsync(token);

            _logger.LogInformation("Retrieved {Count} honors for pathfinder with ID {PathfinderId}", pathfinderhonors.Count, pathfinderId);
            return _mapper.Map<ICollection<Outgoing.PathfinderHonorDto>>(pathfinderhonors);
        }

        public async Task<ICollection<Outgoing.PathfinderHonorDto>> GetAllByStatusAsync(string honorStatus, CancellationToken token)
        {
            _logger.LogInformation("Getting all honors with status {HonorStatus}", honorStatus);
            
            if (string.IsNullOrWhiteSpace(honorStatus))
            {
                _logger.LogWarning("Empty honor status provided");
                return new List<Outgoing.PathfinderHonorDto>();
            }

            var pathfinderhonors = await _dbContext.PathfinderHonors
                .Include(phs => phs.PathfinderHonorStatus)
                .Include(h => h.Honor)
                .Where(phs => phs.PathfinderHonorStatus.Status.ToLower() == honorStatus.ToLower())
                .OrderBy(ph => ph.Honor.Name)
                .ToListAsync(token);

            _logger.LogInformation("Retrieved {Count} honors with status {HonorStatus}", pathfinderhonors.Count, honorStatus);
            return _mapper.Map<ICollection<Outgoing.PathfinderHonorDto>>(pathfinderhonors);
        }

        public async Task<Outgoing.PathfinderHonorDto> GetByIdAsync(Guid pathfinderId, Guid honorId, CancellationToken token)
        {
            _logger.LogInformation("Getting honor with ID {HonorId} for pathfinder with ID {PathfinderId}", honorId, pathfinderId);
            
            PathfinderHonor entity = await GetFilteredPathfinderHonors(pathfinderId, honorId, token)
                .Include(phs => phs.PathfinderHonorStatus)
                .Include(h => h.Honor)
                .SingleOrDefaultAsync(cancellationToken: token);

            if (entity == default)
            {
                _logger.LogWarning("Honor with ID {HonorId} not found for pathfinder with ID {PathfinderId}", honorId, pathfinderId);
            }
            else
            {
                _logger.LogInformation("Retrieved honor with ID {HonorId} for pathfinder with ID {PathfinderId}", honorId, pathfinderId);
            }

            return entity == default
                ? default
                : _mapper.Map<Outgoing.PathfinderHonorDto>(entity);
        }

        public async Task<Outgoing.PathfinderHonorDto> AddAsync(Guid pathfinderId, Incoming.PostPathfinderHonorDto incomingPathfinderHonor, CancellationToken token)
        {
            _logger.LogInformation("Adding honor for pathfinder with ID {PathfinderId}", pathfinderId);
            try
            {
                Incoming.PathfinderHonorDto newPathfinderHonor = await MapStatus(pathfinderId, incomingPathfinderHonor, token);

                await _validator.ValidateAsync(
                    newPathfinderHonor,
                    opts => opts.ThrowOnFailures()
                            .IncludeRulesNotInRuleSet()
                            .IncludeRuleSets("post"),
                    token);

                var newEntity = _mapper.Map<PathfinderHonor>(newPathfinderHonor);

                if (newPathfinderHonor.Status == HonorStatus.Earned.ToString())
                {
                    newEntity.Earned = DateTime.UtcNow;
                }

                await _dbContext.AddAsync(newEntity, token);
                await _dbContext.SaveChangesAsync(token);
                _logger.LogInformation("Added honor with ID {HonorId} for pathfinder with ID {PathfinderId}", newEntity.HonorID, pathfinderId);

                var createdEntity = await GetFilteredPathfinderHonors(newEntity.PathfinderID, newEntity.HonorID, token)
                    .Include(phs => phs.PathfinderHonorStatus)
                    .Include(h => h.Honor)
                    .SingleOrDefaultAsync(cancellationToken: token);

                return _mapper.Map<Outgoing.PathfinderHonorDto>(createdEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding honor for pathfinder with ID {PathfinderId} and honor name {HonorName}", pathfinderId, incomingPathfinderHonor.HonorName);
                throw new InvalidOperationException($"Failed to add honor for pathfinder with ID {pathfinderId} and honor name {incomingPathfinderHonor.HonorName}", ex);
            }
        }

        public async Task<Outgoing.PathfinderHonorDto> UpdateAsync(Guid pathfinderId, Guid honorId, Incoming.PutPathfinderHonorDto incomingPathfinderHonor, CancellationToken token)
        {
            _logger.LogInformation("Updating honor with ID {HonorId} for pathfinder with ID {PathfinderId}", honorId, pathfinderId);
            try
            {
                var targetPathfinderHonor = await _dbContext.PathfinderHonors
                                                .Where(p => p.PathfinderID == pathfinderId && p.HonorID == honorId)
                                                .Include(phs => phs.PathfinderHonorStatus)
                                                .Include(h => h.Honor)
                                                .SingleOrDefaultAsync(token);

                if (targetPathfinderHonor == default)
                {
                    _logger.LogWarning("Honor with ID {HonorId} not found for pathfinder with ID {PathfinderId}", honorId, pathfinderId);
                    return default;
                }

                Incoming.PathfinderHonorDto updatedPathfinderHonor = await MapStatus(pathfinderId, incomingPathfinderHonor, token, honorId);

                await _validator.ValidateAsync(
                    updatedPathfinderHonor,
                    opts => opts.ThrowOnFailures()
                            .IncludeRulesNotInRuleSet(),
                    token);

                targetPathfinderHonor.StatusCode = updatedPathfinderHonor.StatusCode;

                if (updatedPathfinderHonor.Status == HonorStatus.Earned.ToString())
                {
                    targetPathfinderHonor.Earned = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync(token);
                _logger.LogInformation("Updated honor with ID {HonorId} for pathfinder with ID {PathfinderId}", honorId, pathfinderId);

                return _mapper.Map<Outgoing.PathfinderHonorDto>(targetPathfinderHonor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating honor with ID {HonorId} for pathfinder with ID {PathfinderId} and status {Status}", honorId, pathfinderId, incomingPathfinderHonor.Status);
                throw new InvalidOperationException($"Failed to update honor with ID {honorId} for pathfinder with ID {pathfinderId} and status {incomingPathfinderHonor.Status}", ex);
            }
        }

        private IQueryable<PathfinderHonor> GetFilteredPathfinderHonors(Guid pathfinderId, Guid honorId, CancellationToken token)
        {
            return _dbContext.PathfinderHonors
                .Where(p => p.PathfinderID == pathfinderId)
                .Where(ph => ph.HonorID == honorId);
        }

        private async Task<Incoming.PathfinderHonorDto> MapStatus(Guid pathfinderId, dynamic upsertPathfinderHonor, CancellationToken token,
                            Guid honorId = new Guid())
        {
            _logger.LogInformation("Mapping status for honor with ID {HonorId} for pathfinder with ID {PathfinderId}", honorId, pathfinderId);
            
            Enum.TryParse(upsertPathfinderHonor.Status, true, out HonorStatus statusEntity);

            try
            {
                _honorStatus = await _dbContext.PathfinderHonorStatuses
                    .Where(s => s.Status == statusEntity.ToString())
                    .SingleAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Status {Status} not found, using Unknown status", statusEntity.ToString());
                _honorStatus = new()
                {
                    Status = "Unknown",
                    StatusCode = -1
                };
            }

            if (honorId == Guid.Empty)
            {
                honorId = upsertPathfinderHonor.HonorID;
            }

            Incoming.PathfinderHonorDto mappedPathfinderHonor = new()
            {
                HonorID = honorId,
                PathfinderID = pathfinderId,
                Status = _honorStatus.Status,
                StatusCode = _honorStatus.StatusCode
            };

            return mappedPathfinderHonor;
        }
    }
}
