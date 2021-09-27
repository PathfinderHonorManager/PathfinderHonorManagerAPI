using System;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Validators
{
    public class PathfinderValidator : AbstractValidator<PathfinderDto>
    {
        private readonly PathfinderContext _dbContext;

        public PathfinderValidator(PathfinderContext dbContext)
        {
            _dbContext = dbContext;
            SetUpValidation();
        }

        private void SetUpValidation()
        {
            RuleFor(p => p.FirstName).NotEmpty();
            RuleFor(p => p.LastName).NotEmpty();
            RuleFor(p => p.Email).NotEmpty();
            RuleFor(p => p.Email)
                .MustAsync(async (email, token) => !await _dbContext.Pathfinders.AnyAsync(p => p.Email == email, token))
                .WithMessage(
                    p => $"Pathfinder email address ({p.Email}) is taken.");
        }
    }
}
