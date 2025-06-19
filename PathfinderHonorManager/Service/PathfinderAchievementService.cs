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
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;
using FluentValidation.Results;

namespace PathfinderHonorManager.Service
{
    public class PathfinderAchievementService : IPathfinderAchievementService
    {
        private readonly PathfinderContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<PathfinderAchievementService> _logger;
        private readonly IValidator<Incoming.PathfinderAchievementDto> _validator;


        public PathfinderAchievementService(
            PathfinderContext dbContext,
            IMapper mapper,
            ILogger<PathfinderAchievementService> logger,
            IValidator<Incoming.PathfinderAchievementDto> validator)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<ICollection<Outgoing.PathfinderAchievementDto>> GetAllAsync(bool showAllAchievements = false, CancellationToken token = default)
        {
            _logger.LogInformation($"Getting all pathfinder achievements, showAllAchievements: {showAllAchievements}");
            IQueryable<PathfinderAchievement> query = _dbContext.PathfinderAchievements
                .Include(a => a.Achievement)
                .Include(c => c.Achievement.PathfinderClass)
                .Include(pa => pa.Achievement.Category)
                .Include(p => p.Pathfinder);

            if (!showAllAchievements)
            {
                query = query.Where(pa => pa.Pathfinder.Grade != null && pa.Achievement.Grade == pa.Pathfinder.Grade);
            }

            var achievements = await query
                .OrderBy(pa => pa.PathfinderID)
                .ThenBy(pa => pa.Achievement.Grade)
                .ThenBy(pa => pa.Achievement.Category.CategorySequenceOrder)
                .ThenBy(pa => pa.Achievement.Level)
                .ThenBy(pa => pa.Achievement.AchievementSequenceOrder)
                .ToListAsync(token);

            return _mapper.Map<ICollection<Outgoing.PathfinderAchievementDto>>(achievements);
        }

        public async Task<Outgoing.PathfinderAchievementDto> GetByIdAsync(Guid pathfinderId, Guid achievementId, CancellationToken token)
        {
            _logger.LogInformation($"Getting pathfinder achievement by Pathfinder ID: {pathfinderId} Achievement ID {achievementId}");
            var pathfinderAchievement = await _dbContext.PathfinderAchievements
                .Include(a => a.Achievement)
                .Include(a => a.Achievement.Category)
                .Where(pa => pa.PathfinderID == pathfinderId && pa.AchievementID == achievementId)
                .SingleOrDefaultAsync(token);

            return pathfinderAchievement == null ? null : _mapper.Map<Outgoing.PathfinderAchievementDto>(pathfinderAchievement);
        }
        public async Task<ICollection<Outgoing.PathfinderAchievementDto>> GetAllAchievementsForPathfinderAsync(Guid pathfinderId, bool showAllAchievements = false, CancellationToken token = default)
        {
            _logger.LogInformation($"Getting all achievements for Pathfinder ID: {pathfinderId}, showAllAchievements: {showAllAchievements}");

            IQueryable<PathfinderAchievement> query = _dbContext.PathfinderAchievements
                .Where(pa => pa.PathfinderID == pathfinderId)
                .Include(pa => pa.Achievement)
                .Include(c => c.Achievement.PathfinderClass)
                .Include(pa => pa.Achievement.Category)
                .OrderBy(pa => pa.Achievement.Grade)
                .ThenBy(pa => pa.Achievement.Category.CategorySequenceOrder)
                .ThenBy(pa => pa.Achievement.Level)
                .ThenBy(pa => pa.Achievement.AchievementSequenceOrder);

            if (!showAllAchievements)
            {
                var pathfinder = await _dbContext.Pathfinders
                    .FirstOrDefaultAsync(p => p.PathfinderID == pathfinderId, token);

                if (pathfinder?.Grade != null)
                {
                    query = query.Where(pa => pa.Achievement.Grade == pathfinder.Grade);
                }
            }

            var achievements = await query.ToListAsync(token);

            return _mapper.Map<ICollection<Outgoing.PathfinderAchievementDto>>(achievements);
        }
        public async Task<Outgoing.PathfinderAchievementDto> AddAsync(Guid pathfinderId, Incoming.PostPathfinderAchievementDto achievementId, CancellationToken token)
        {
            Incoming.PathfinderAchievementDto newPathfinderAchievement = new Incoming.PathfinderAchievementDto
            {
                PathfinderID = pathfinderId,
                AchievementID = achievementId.AchievementID
            };

            await _validator.ValidateAsync(newPathfinderAchievement, opts =>
            {
                opts.ThrowOnFailures();
                opts.IncludeRulesNotInRuleSet();
                opts.IncludeRuleSets("post");
            }, token);

            var newEntity = _mapper.Map<PathfinderAchievement>(newPathfinderAchievement);

            await _dbContext.PathfinderAchievements.AddAsync(newEntity, token);
            await _dbContext.SaveChangesAsync(token);

            return await GetByIdAsync(pathfinderId, newEntity.AchievementID, token);
        }

        public async Task<Outgoing.PathfinderAchievementDto> UpdateAsync(Guid pathfinderId, Guid achievementId, Incoming.PutPathfinderAchievementDto updatedAchievement, CancellationToken token)
        {
            var pathfinderAchievement = await _dbContext.PathfinderAchievements
                .FirstOrDefaultAsync(pa => pa.PathfinderID == pathfinderId && pa.AchievementID == achievementId, token);

            if (pathfinderAchievement == null)
            {
                return null;
            }
            pathfinderAchievement.IsAchieved = updatedAchievement.IsAchieved;
            var dto = _mapper.Map<Incoming.PathfinderAchievementDto>(pathfinderAchievement);
            await _validator.ValidateAsync(dto, opts => opts.ThrowOnFailures(), token);
            await _dbContext.SaveChangesAsync(token);

            return _mapper.Map<Outgoing.PathfinderAchievementDto>(pathfinderAchievement);
        }

        public async Task<ICollection<Outgoing.PathfinderAchievementDto>> AddAchievementsForPathfinderAsync(Guid pathfinderId, CancellationToken token)
        {
            var newAchievements = new List<PathfinderAchievement>();

            var pathfinder = await _dbContext.Pathfinders
                .FirstOrDefaultAsync(p => p.PathfinderID == pathfinderId, token);

            if (pathfinder == null)
            {
                _logger.LogError($"Pathfinder with ID {pathfinderId} not found.");
                var failures = new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(Incoming.PathfinderAchievementDto.PathfinderID), $"Pathfinder with ID {pathfinderId} not found.")
                };
                throw new ValidationException("Validation error occurred.", failures);
            }

            var gradeAchievements = await _dbContext.Achievements
                .Where(a => a.Grade == pathfinder.Grade)
                .ToListAsync(token);

            foreach (var achievement in gradeAchievements)
            {
                Incoming.PathfinderAchievementDto newPathfinderAchievement = new Incoming.PathfinderAchievementDto
                {
                    PathfinderID = pathfinderId,
                    AchievementID = achievement.AchievementID
                };

                await _validator.ValidateAsync(newPathfinderAchievement, opts =>
                {
                    opts.ThrowOnFailures();
                    opts.IncludeRulesNotInRuleSet();
                    opts.IncludeRuleSets("post");
                }, token);

                var newEntity = _mapper.Map<PathfinderAchievement>(newPathfinderAchievement);
                newAchievements.Add(newEntity);
            }

            if (newAchievements.Any())
            {
                await _dbContext.PathfinderAchievements.AddRangeAsync(newAchievements, token);
                await _dbContext.SaveChangesAsync(token);
            }

            return _mapper.Map<ICollection<Outgoing.PathfinderAchievementDto>>(newAchievements);
        }
    }
}
