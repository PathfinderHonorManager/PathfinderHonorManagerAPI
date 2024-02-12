using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Incoming;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PathfinderHonorManager.Validators
{
    public class PathfinderAchievementValidator : AbstractValidator<PathfinderAchievementDto>
    {
        private readonly PathfinderContext _dbContext;

        public PathfinderAchievementValidator(PathfinderContext dbContext)
        {
            _dbContext = dbContext;

            RuleFor(dto => dto)
                .MustAsync(async (dto, cancellation) =>
                {
                    var pathfinder = await _dbContext.Pathfinders.FindAsync(new object[] { dto.PathfinderID }, cancellation);
                    var achievement = await _dbContext.Achievements.FindAsync(new object[] { dto.AchievementID }, cancellation);

                    return pathfinder != null && achievement != null && pathfinder.Grade == achievement.Grade;
                })
                .WithMessage("The grade of the pathfinder must match the grade on the achievement.");

            RuleFor(dto => dto)
                .MustAsync(async (dto, cancellation) =>
                {
                    return !await _dbContext.PathfinderAchievements
                        .AnyAsync(pa => pa.PathfinderID == dto.PathfinderID && pa.AchievementID == dto.AchievementID, cancellation);
                })
                .WithMessage("A PathfinderAchievement with the same Pathfinder ID and Achievement ID already exists.");
        }
    }
}
