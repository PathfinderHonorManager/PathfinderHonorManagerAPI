﻿using AutoMapper;
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
            var pathfinderhonors = await _dbContext.PathfinderHonors
                .Include(phs => phs.PathfinderHonorStatus)
                .Include(h => h.Honor)
                .Where(p => p.PathfinderID == pathfinderId)
                .OrderBy(ph => ph.Honor.Name)
                .ToListAsync(token);

            return _mapper.Map<ICollection<Outgoing.PathfinderHonorDto>>(pathfinderhonors);

        }

        public async Task<ICollection<Outgoing.PathfinderHonorDto>> GetAllByStatusAsync(string honorStatus, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(honorStatus))
            {
                return new List<Outgoing.PathfinderHonorDto>();
            }

            var pathfinderhonors = await _dbContext.PathfinderHonors
                .Include(phs => phs.PathfinderHonorStatus)
                .Include(h => h.Honor)
                .Where(phs => phs.PathfinderHonorStatus.Status.ToLower() == honorStatus.ToLower())
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
                                                .Where(p => p.PathfinderID == pathfinderId && p.HonorID == honorId)
                                                .Include(phs => phs.PathfinderHonorStatus)
                                                .Include(h => h.Honor)
                                                .SingleOrDefaultAsync(token);

            if (targetPathfinderHonor == default)
            {
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

            return _mapper.Map<Outgoing.PathfinderHonorDto>(targetPathfinderHonor);
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


            Enum.TryParse(upsertPathfinderHonor.Status, true, out HonorStatus statusEntity);

            try
            {
                _honorStatus = await _dbContext.PathfinderHonorStatuses
                    .Where(s => s.Status == statusEntity.ToString())
                    .SingleAsync(token);
            }
            catch (Exception)
            {
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
