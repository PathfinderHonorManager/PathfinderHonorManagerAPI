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

        private readonly IClubService _clubService;

        private readonly IMapper _mapper;

        private readonly ILogger _logger;

        private readonly IValidator<Incoming.PathfinderDtoInternal> _validator;

        private IQueryable<Pathfinder> QueryPathfindersWithIncludesAsync(string clubCode, bool showInactive)
        {
            var query = _dbContext.Pathfinders
                .Include(pc => pc.PathfinderClass)
                .Include(ph => ph.PathfinderHonors)
                    .ThenInclude(phs => phs.PathfinderHonorStatus)
                .Include(ph => ph.PathfinderHonors.OrderBy(x => x.Honor.Name))
                    .ThenInclude(h => h.Honor)
                .Include(c => c.Club)
                .Where(c => c.Club.ClubCode == clubCode);

            if (!showInactive)
            {
                query = query.Where(p => (bool)p.IsActive);
            }

            return query;

        }

        private IQueryable<Pathfinder> QueryPathfinderByIdAsync(Guid pathfinderId, string clubCode)
        {
            return _dbContext.Pathfinders
                .Include(pc => pc.PathfinderClass)
                 .Include(c => c.Club)
                .Where(c => c.Club.ClubCode == clubCode && c.PathfinderID == pathfinderId);
        }

        public PathfinderService(
            PathfinderContext context,
            IClubService clubService,
            IMapper mapper,
            IValidator<Incoming.PathfinderDtoInternal> validator,
            ILogger<PathfinderService> logger)
        {
            _dbContext = context;
            _clubService = clubService;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<ICollection<Outgoing.PathfinderDependantDto>> GetAllAsync(string clubCode, bool showInactive, CancellationToken token)
        {
            List<Pathfinder> pathfinders = await QueryPathfindersWithIncludesAsync(clubCode, showInactive)
                .OrderBy(p => p.Grade)
                .ThenBy(p => p.LastName)
                .ToListAsync(token);

            return _mapper.Map<ICollection<Outgoing.PathfinderDependantDto>>(pathfinders);
        }

        public async Task<Outgoing.PathfinderDependantDto> GetByIdAsync(Guid id, string clubCode, CancellationToken token)
        {
            Pathfinder entity;
            bool showInactive = true;

            entity = await QueryPathfindersWithIncludesAsync(clubCode, showInactive)
                .SingleOrDefaultAsync(p => p.PathfinderID == id, token);

            return entity == default
                ? default
                : _mapper.Map<Outgoing.PathfinderDependantDto>(entity);
        }


        public async Task<Outgoing.PathfinderDto> AddAsync(Incoming.PathfinderDto newPathfinder, string clubCode, CancellationToken token)
        {
            var club = await _clubService.GetByCodeAsync(clubCode, token);

            var newPathfinderWithClubId = new Incoming.PathfinderDtoInternal()
            {
                FirstName = newPathfinder.FirstName,
                LastName = newPathfinder.LastName,
                Email = newPathfinder.Email,
                Grade = newPathfinder.Grade,
                ClubID = club?.ClubID ?? Guid.Empty
        };

            await _validator.ValidateAsync(
                newPathfinderWithClubId,
                opts => opts.ThrowOnFailures()
                        .IncludeAllRuleSets(),
                token);

            var newEntity = _mapper.Map<Pathfinder>(newPathfinderWithClubId);

            await _dbContext.AddAsync(newEntity, token);
            await _dbContext.SaveChangesAsync(token);
            _logger.LogInformation($"Pathfinder(Id: {newEntity.PathfinderID} added to database.");

            var createdPathfinder = await GetByIdAsync(newEntity.PathfinderID, clubCode, token);

            return _mapper.Map<Outgoing.PathfinderDto>(createdPathfinder);
        }

        public async Task<Outgoing.PathfinderDto> UpdateAsync(Guid pathfinderId, Incoming.PutPathfinderDto updatedPathfinder, string clubCode, CancellationToken token)
        {
            Pathfinder targetPathfinder;
            targetPathfinder = await QueryPathfinderByIdAsync(pathfinderId, clubCode)
                                        .SingleOrDefaultAsync(token);

            var club = await _clubService.GetByCodeAsync(clubCode, token);

            if (targetPathfinder == default)
            {
                return default;
            }

            Incoming.PathfinderDtoInternal mappedPathfinder = new()
            {
                FirstName = targetPathfinder.FirstName,
                LastName = targetPathfinder.LastName,
                Email = targetPathfinder.Email,
                Grade = updatedPathfinder.Grade,
                IsActive = updatedPathfinder.IsActive,
                ClubID = club.ClubID
            };

            await _validator.ValidateAsync(
                mappedPathfinder,
                opts => opts.ThrowOnFailures(),
                token);

            if (mappedPathfinder.Grade != null)
            {
                targetPathfinder.Grade = mappedPathfinder.Grade;
            }
            if (mappedPathfinder.IsActive.HasValue)
            {
                targetPathfinder.IsActive = mappedPathfinder.IsActive;
            }

            await _dbContext.SaveChangesAsync(token);

            return _mapper.Map<Outgoing.PathfinderDto>(targetPathfinder);

        }
    }
}
