using System;

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
            //RuleFor(ph => ph.Status).NotEmpty();
            RuleFor(p => p.HonorID)
                .MustAsync(async (honorid, token) => !await _dbContext.PathfinderHonors.AnyAsync(h => h.HonorID == honorid, token))
                .WithMessage(
                    p => $"Pathfinder honor already added.");
        }


    }
}
