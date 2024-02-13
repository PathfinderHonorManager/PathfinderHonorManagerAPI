using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Validators
{
    public class PathfinderAchievementValidator : AbstractValidator<PathfinderAchievementDto>
    {
        private readonly PathfinderContext _dbContext;

        public PathfinderAchievementValidator(PathfinderContext dbContext)
        {
            _dbContext = dbContext;
            RuleSet(
                "post",
                () =>
            {
                RuleFor(dto => dto)
                    .MustAsync(async (dto, cancellation) =>
                    {
                        return !await _dbContext.PathfinderAchievements
                            .AnyAsync(pa => pa.PathfinderID == dto.PathfinderID && pa.AchievementID == dto.AchievementID, cancellation);
                    })
                    .WithName(nameof(PathfinderAchievementDto.AchievementID))
                    .WithMessage(dto => $"Pathfinder {dto.PathfinderID} already has been assigned achievement {dto.AchievementID}");
                RuleFor(dto => dto)
                    .MustAsync(async (dto, cancellation) =>
                    {
                        var pathfinder = await _dbContext.Pathfinders.FindAsync(new object[] { dto.PathfinderID }, cancellation);
                        var achievement = await _dbContext.Achievements.FindAsync(new object[] { dto.AchievementID }, cancellation);

                        if (pathfinder != null && achievement != null)
                        {
                            return pathfinder.Grade == achievement.Grade;
                        }

                        return true;
                    })
                    .WithName(nameof(PathfinderAchievementDto.AchievementID))
                    .WithMessage("The pathfinder's grade must match the achievement.");
                RuleFor(p => p)
                    .MustAsync(
                        async (dto, token) =>
                            await _dbContext.Pathfinders.AnyAsync(p => p.PathfinderID == dto.PathfinderID, token))
                    .WithName(nameof(PathfinderAchievementDto.PathfinderID))
                    .WithMessage(dto => $"Invalid Pathfinder ID {dto.PathfinderID} provided.");
                RuleFor(a => a)
                    .MustAsync(
                        async (dto, token) =>
                            await _dbContext.Achievements.AnyAsync(a => a.AchievementID == dto.AchievementID, token))
                    .WithName(nameof(PathfinderAchievementDto.AchievementID))
                    .WithMessage(dto => $"Invalid Achievement ID {dto.AchievementID} provided.");
            });
        }
    }
}
