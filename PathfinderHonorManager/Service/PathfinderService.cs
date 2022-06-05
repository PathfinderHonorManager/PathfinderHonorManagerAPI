using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Service
{
    public class PathfinderService : IPathfinderService
    {
        private readonly PathfinderContext _dbContext;

        private readonly IMapper _mapper;

        private readonly ILogger _logger;

        private readonly IValidator<Incoming.PathfinderDto> _validator;


        public PathfinderService(
            PathfinderContext context,
            IMapper mapper,
            IValidator<Incoming.PathfinderDto> validator,
            ILogger<PathfinderService> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<ICollection<Outgoing.PathfinderDependantDto>> GetAllAsync(CancellationToken token)
        {
            List<Pathfinder> pathfinders = await _dbContext.Pathfinders
                .Include(pc => pc.PathfinderClass)
                .Include(ph => ph.PathfinderHonors)
                    .ThenInclude(phs => phs.PathfinderHonorStatus)
                .Include(ph => ph.PathfinderHonors.OrderBy(x => x.Honor.Name))
                    .ThenInclude(h => h.Honor)
                .ToListAsync(token);

            return _mapper.Map<ICollection<Outgoing.PathfinderDependantDto>>(pathfinders);

        }

        public async Task<Outgoing.PathfinderDependantDto> GetByIdAsync(Guid id, CancellationToken token)
        {
            Pathfinder entity;

            entity = await _dbContext.Pathfinders
                .Include(pc => pc.PathfinderClass)
                .Include(ph => ph.PathfinderHonors)
                    .ThenInclude(phs => phs.PathfinderHonorStatus)
                .Include(ph => ph.PathfinderHonors.OrderBy(x => x.Honor.Name))
                    .ThenInclude(h => h.Honor)
                .SingleOrDefaultAsync(p => p.PathfinderID == id, token);

            return entity == default
                ? default
                : _mapper.Map<Outgoing.PathfinderDependantDto>(entity);
        }

        public async Task<Outgoing.PathfinderDto> AddAsync(Incoming.PathfinderDto newPathfinder, CancellationToken token)
        {
            await _validator.ValidateAsync(
                newPathfinder,
                opts => opts.ThrowOnFailures()
                        .IncludeAllRuleSets(),
                token);

            var newEntity = _mapper.Map<Pathfinder>(newPathfinder);

            await _dbContext.AddAsync(newEntity, token);
            await _dbContext.SaveChangesAsync(token);
            _logger.LogInformation($"Pathfinder(Id: {newEntity.PathfinderID} added to database.");

            var createdPathfinder = await GetByIdAsync(newEntity.PathfinderID, token);

            return _mapper.Map<Outgoing.PathfinderDto>(createdPathfinder);
        }

        public async Task<Outgoing.PathfinderDto> UpdateAsync(Guid pathfinderId, Incoming.PutPathfinderDto updatedPathfinder, CancellationToken token)
        {
            var targetPathfinder = await _dbContext.Pathfinders
                .Where(p => p.PathfinderID == pathfinderId)
                .Include(pc => pc.PathfinderClass)
                .SingleOrDefaultAsync(token);

            if (targetPathfinder == default)
            {
                return default;
            }

            Incoming.PathfinderDto mappedPathfinder = new()
            {
                FirstName = targetPathfinder.FirstName,
                LastName = targetPathfinder.LastName,
                Email = targetPathfinder.Email,
                Grade = updatedPathfinder.Grade
            };


            await _validator.ValidateAsync(
                mappedPathfinder,
                opts => opts.ThrowOnFailures(),
                token);

            targetPathfinder.Grade = mappedPathfinder.Grade;

            await _dbContext.SaveChangesAsync(token);

            return _mapper.Map<Outgoing.PathfinderDto>(targetPathfinder);      

        }

    }
}
