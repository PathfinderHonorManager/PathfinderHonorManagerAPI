using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace PathfinderHonorManager.Tests.Helpers;

public class DummyValidator<T> : AbstractValidator<T>
{
    public override ValidationResult Validate(ValidationContext<T> context)
    {
        return new ValidationResult(new List<ValidationFailure>());
    }

    public override Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ValidationResult(new List<ValidationFailure>()));
    }
}
