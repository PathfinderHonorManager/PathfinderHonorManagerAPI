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
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Service.Interfaces;
using System.Collections.Generic;
using System.Linq;

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

        public async Task<ICollection<Outgoing.PathfinderDto>> GetAllAsync(CancellationToken token)
        {
            var pathfinders = await _dbContext.Pathfinders
                .ToListAsync(token);

            return _mapper.Map<ICollection<Outgoing.PathfinderDto>>(pathfinders);

        }

        public async Task<Outgoing.PathfinderDependantDto> GetByIdAsync(Guid id, CancellationToken token)
        {
            Pathfinder entity;

            entity = await _dbContext.Pathfinders
                .Include(ph => ph.PathfinderHonors)
                    .ThenInclude(phs => phs.PathfinderHonorStatus)
                .Include(ph => ph.PathfinderHonors)
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
                opts => opts.ThrowOnFailures(),
                token);

            var newEntity = _mapper.Map<Pathfinder>(newPathfinder);

            await _dbContext.AddAsync(newEntity, token);
            await _dbContext.SaveChangesAsync(token);
            _logger.LogInformation($"Pathfinder(Id: {newEntity.PathfinderID} added to database.");

            return _mapper.Map<Outgoing.PathfinderDto>(newEntity);
        }

    }
}
