using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PathfinderHonorManager.DataAccess;
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

        public PathfinderAchievementService(PathfinderContext dbContext, IMapper mapper, ILogger<PathfinderAchievementService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ICollection<PathfinderAchievementDto>> GetAllAsync(CancellationToken token)
        {
            _logger.LogInformation("Getting all pathfinder achievements");
            var achievements = await _dbContext.PathfinderAchievements
                .ToListAsync(token);

            return _mapper.Map<ICollection<PathfinderAchievementDto>>(achievements);
        }

        public async Task<PathfinderAchievementDto> GetByIdAsync(Guid id, CancellationToken token)
        {
            _logger.LogInformation($"Getting pathfinder achievement by ID: {id}");
            var pathfinderAchievement = await _dbContext.PathfinderAchievements
                .SingleOrDefaultAsync(a => a.PathfinderAchievementID == id, token);

            return pathfinderAchievement == null ? null : _mapper.Map<PathfinderAchievementDto>(pathfinderAchievement);
        }
    }
}
