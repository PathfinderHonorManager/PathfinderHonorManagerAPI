using System;
using FluentValidation;
using PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Validators
{
    public class HonorValidator : AbstractValidator<HonorDto>
    {
        public HonorValidator()
        {
            RuleFor(h => h.Name).NotEmpty();
            RuleFor(h => h.Level).InclusiveBetween(1, 3);
            RuleFor(h => h.WikiPath)
                .Must(IsValidUri)
                .WithMessage("'WikiPath' must be a valid URL.");
            RuleFor(h => h.PatchFilename).NotEmpty().Matches(@"\.(gif|jpe?g|png)$")
                .WithMessage("'PatchFilename' must be a gif, jpg or png.");
        }

        private bool IsValidUri(Uri uri)
        {
            return Uri.TryCreate(uri.ToString(), UriKind.Absolute, out Uri result)
                && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}
