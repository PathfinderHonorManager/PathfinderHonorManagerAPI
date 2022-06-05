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

        private readonly ILogger _logger;

        private readonly IValidator<Incoming.PathfinderHonorDto> _validator;

        private int newStatusCode { get; set; }

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
            var pathfinderhonors = await _dbContext.PathfinderHonors
                .Include(phs => phs.PathfinderHonorStatus)
                .Include(h => h.Honor)
                .Where(p => p.PathfinderID == pathfinderId )
                .OrderBy(ph => ph.Honor.Name)
                .ToListAsync(token);

            return _mapper.Map<ICollection<Outgoing.PathfinderHonorDto>>(pathfinderhonors);

        }

        public async Task<Outgoing.PathfinderHonorDto> GetByIdAsync(Guid pathfinderId, Guid honorId, CancellationToken token)
        {
            PathfinderHonor entity;

            entity = await GetFilteredPathfinderHonors(pathfinderId, honorId, token)
                .Include(phs => phs.PathfinderHonorStatus)
                .Include(h => h.Honor)
                .SingleOrDefaultAsync(cancellationToken: token);

            return entity == default
                ? default
                : _mapper.Map<Outgoing.PathfinderHonorDto>(entity);
        }

        public async Task<Outgoing.PathfinderHonorDto> AddAsync(Guid pathfinderId, Incoming.PostPathfinderHonorDto incomingPathfinderHonor, CancellationToken token)
        {

            Incoming.PathfinderHonorDto newPathfinderHonor = MapStatus(pathfinderId, incomingPathfinderHonor);


            await _validator.ValidateAsync(
                newPathfinderHonor,
                opts => opts.ThrowOnFailures()
                            .IncludeRulesNotInRuleSet()
                            .IncludeRuleSets("post"),
                        token);

            var newEntity = _mapper.Map<PathfinderHonor>(newPathfinderHonor);

            await _dbContext.AddAsync(newEntity, token);
            await _dbContext.SaveChangesAsync(token);
            _logger.LogInformation($"Pathfinder honor(Id: {newEntity.PathfinderHonorID} added to database.");

            var createdEntity = await GetFilteredPathfinderHonors(newEntity.PathfinderID, newEntity.HonorID, token)
                .Include(phs => phs.PathfinderHonorStatus)
                .Include(h => h.Honor)
                .SingleOrDefaultAsync(cancellationToken: token);

            return _mapper.Map<Outgoing.PathfinderHonorDto>(createdEntity);
        }

        public async Task<Outgoing.PathfinderHonorDto> UpdateAsync(Guid pathfinderId, Guid honorId, Incoming.PutPathfinderHonorDto incomingPathfinderHonor, CancellationToken token)
        {
            var targetPathfinderHonor = await _dbContext.PathfinderHonors
                                                .Where(p => p.PathfinderID == pathfinderId && p.HonorID ==honorId)
                                                .Include(phs => phs.PathfinderHonorStatus)
                                                .Include(h => h.Honor)
                                                .SingleOrDefaultAsync(token);

            if (targetPathfinderHonor == default)
            {
                return default;
            }

            Incoming.PathfinderHonorDto updatedPathfinderHonor = MapStatus(pathfinderId, incomingPathfinderHonor, honorId);

            await _validator.ValidateAsync(
                updatedPathfinderHonor,
                opts => opts.ThrowOnFailures()
                            .IncludeRulesNotInRuleSet(),
                        token);

            targetPathfinderHonor.StatusCode = updatedPathfinderHonor.StatusCode;

            await _dbContext.SaveChangesAsync(token);

            return _mapper.Map<Outgoing.PathfinderHonorDto>(targetPathfinderHonor);
        }

        private IQueryable<PathfinderHonor> GetFilteredPathfinderHonors(Guid pathfinderId, Guid honorId, CancellationToken token)
        {
            return _dbContext.PathfinderHonors
                .Where(p => p.PathfinderID == pathfinderId)
                .Where(ph => ph.HonorID == honorId);
        }

        private Incoming.PathfinderHonorDto MapStatus(Guid pathfinderId, dynamic upsertPathfinderHonor, Guid honorId = new Guid())
        {
            if (Enum.TryParse(upsertPathfinderHonor.Status, out HonorStatus statusEntity))
            {
                newStatusCode = statusEntity switch
                {
                    HonorStatus.Awarded => (int)HonorStatus.Awarded,
                    HonorStatus.Earned => (int)HonorStatus.Earned,
                    HonorStatus.Planned => (int)HonorStatus.Planned,
                    _ => -1,
                };
            }
            else
            {
                newStatusCode = -1;
            }

            if (honorId == Guid.Empty)
            {
                honorId = upsertPathfinderHonor.HonorID;
            }

            Incoming.PathfinderHonorDto mappedPathfinderHonor = new()
            {
                HonorID = honorId,
                PathfinderID = pathfinderId,
                Status = upsertPathfinderHonor.Status,
                StatusCode = newStatusCode
            };

            return mappedPathfinderHonor;
        }
    }
}
