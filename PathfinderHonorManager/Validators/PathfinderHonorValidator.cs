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
            RuleFor(p => p)
                .MustAsync(
                    async (dto, token) =>
                         !await _dbContext.PathfinderHonors
                            .Where(p => p.HonorID == dto.HonorID)
                            .Where(p => p.PathfinderID == dto.PathfinderID)
                            .AnyAsync()
                    )
                .WithName(nameof(PathfinderHonorDto.HonorID))
                .WithMessage(
                    dto => $"Pathfinder {dto.PathfinderID} already has honor {dto.HonorID}.");
        }


    }
}
