using System;
using System.Linq;
using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Validators
{
    public class PathfinderHonorValidator : AbstractValidator<PathfinderHonorDto>
    {
        private readonly PathfinderContext _dbContext;

        public PathfinderHonorValidator(PathfinderContext dbContext)
        {
            _dbContext = dbContext;
            SetUpValidation();
        }

        private void SetUpValidation()
        {
            RuleFor(p => p.StatusCode).GreaterThan(0)
                .WithMessage(
                    dto => $"Honor status {dto.Status} is invalid. Valid statuses are: Planned, Earned, Awarded.");
            RuleSet(
                "post",
                () =>
                {
                    RuleFor(p => p)
                    .MustAsync(
                        async (dto, token) =>
                             !await _dbContext.PathfinderHonors
                                .AnyAsync(p => p.HonorID == dto.HonorID && p.PathfinderID == dto.PathfinderID))
                    .WithName(nameof(PathfinderHonorDto.HonorID))
                    .WithMessage(
                        dto => $"Pathfinder {dto.PathfinderID} already has honor {dto.HonorID}.");
                });
        }
    }
}
