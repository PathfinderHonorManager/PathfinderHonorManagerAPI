using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Validators
{
    public class HonorValidator : AbstractValidator<HonorDto>
    {
        private readonly PathfinderContext _dbcontext;

        public HonorValidator(PathfinderContext dbcontext)
        {
            _dbcontext = dbcontext;

            RuleFor(h => h.Name)
                .NotEmpty();
            RuleFor(h => h.Level)
                .InclusiveBetween(1, 3);
            RuleFor(h => h.WikiPath)
                .Must(IsValidUri)
                .WithMessage("'WikiPath' must be a valid URL.");
            RuleFor(h => h.PatchFilename).NotEmpty().Matches(@"\.(gif|jpe?g|png)$")
                .WithMessage("'PatchFilename' must be a gif, jpg or png.");

            RuleSet(
                "post",
                () =>
                {
                    RuleFor(h => h.Name)
                        .MustAsync(BeUniqueName)
                        .WithMessage("'Name' must be unique");
                });
        }

        private bool IsValidUri(Uri uri)
        {
            return Uri.TryCreate(uri.ToString(), UriKind.Absolute, out Uri result)
                && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken token)
        {
            return await _dbcontext.Honors.AllAsync(h => h.Name != name, token);
        }
    }
}
