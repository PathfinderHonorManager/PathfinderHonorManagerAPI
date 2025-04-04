using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Incoming;
using System;
using System.Text.RegularExpressions;

namespace PathfinderHonorManager.Validators
{
    public class ClubValidator : AbstractValidator<ClubDto>
    {
        private readonly PathfinderContext _dbContext;

        public ClubValidator(PathfinderContext dbContext)
        {
            _dbContext = dbContext;
            SetUpValidation();
        }

        private void SetUpValidation()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Club name must not be empty and must not exceed 100 characters.");

            RuleFor(c => c.ClubCode)
                .NotEmpty()
                .Length(4, 20)
                .Matches(new Regex("^[A-Z0-9]+$", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
                .WithMessage("Club code must be between 4 and 20 characters and contain only uppercase letters and numbers.");

            RuleSet(
                "post",
                () =>
                {
                    RuleFor(c => c.ClubCode)
                        .MustAsync(async (code, token) =>
                            !await _dbContext.Clubs.AnyAsync(c => c.ClubCode == code, token))
                        .WithMessage(c => $"Club code {c.ClubCode} is already in use.");
                });
        }
    }
} 