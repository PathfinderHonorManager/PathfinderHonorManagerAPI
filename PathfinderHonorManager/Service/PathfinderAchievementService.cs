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
using PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;

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

        public async Task<ICollection<PathfinderAchievementDto>> GetAllAsync(CancellationToken token)
        {
            _logger.LogInformation("Getting all pathfinder achievements");
            var achievements = await _dbContext.PathfinderAchievements
                .ToListAsync(token);

            return _mapper.Map<ICollection<PathfinderAchievementDto>>(achievements);
        }

        public async Task<PathfinderAchievementDto> GetByIdAsync(Guid pathfinderId, Guid achievementId, CancellationToken token)
        {
            _logger.LogInformation($"Getting pathfinder achievement by Pathfinder ID: {pathfinderId} Achievement ID {achievementId}");
            var pathfinderAchievement = await _dbContext.PathfinderAchievements
                .Where(pa => pa.PathfinderID == pathfinderId && pa.AchievementID == achievementId)
                .SingleOrDefaultAsync(token);

            return pathfinderAchievement == null ? null : _mapper.Map<PathfinderAchievementDto>(pathfinderAchievement);
        }
        public async Task<ICollection<PathfinderAchievementDto>> GetAllAchievementsForPathfinderAsync(Guid pathfinderId, CancellationToken token)
        {
            _logger.LogInformation($"Getting all achievements for Pathfinder ID: {pathfinderId}");
            
            var achievements = await _dbContext.PathfinderAchievements
                .Where(pa => pa.PathfinderID == pathfinderId)
                .Include(pa => pa.Achievement)
                .ToListAsync(token);

            return _mapper.Map<ICollection<PathfinderAchievementDto>>(achievements);
        }
        public async Task<PathfinderAchievementDto> AddAsync(Guid pathfinderId, Incoming.PostPathfinderAchievementDto achievementId, CancellationToken token)
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

        public async Task<PathfinderAchievementDto> UpdateAsync(Guid pathfinderId, Guid achievementId, Incoming.PutPathfinderAchievementDto updatedAchievement, CancellationToken token)
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

            return _mapper.Map<PathfinderAchievementDto>(pathfinderAchievement);
        }

        public async Task<ICollection<PathfinderAchievementDto>> AddAchievementsForGradeAsync(Guid pathfinderId, CancellationToken token)
        {
            var pathfinder = await _dbContext.Pathfinders
                .FirstOrDefaultAsync(p => p.PathfinderID == pathfinderId, token);

            if (pathfinder == null)
            {
                _logger.LogError($"Pathfinder with ID {pathfinderId} not found.");
                return null;
            }

            var gradeAchievements = await _dbContext.Achievements
                .Where(a => a.Grade == pathfinder.Grade)
                .ToListAsync(token);

            var newAchievements = new List<PathfinderAchievement>();

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

            await _dbContext.PathfinderAchievements.AddRangeAsync(newAchievements, token);
            await _dbContext.SaveChangesAsync(token);

            return _mapper.Map<ICollection<PathfinderAchievementDto>>(newAchievements);
        }
    }
}
