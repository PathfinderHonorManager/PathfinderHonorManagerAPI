using System;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Tests.Helpers;
using PathfinderHonorManager.Validators;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Tests.Validator
{
    [TestFixture]
    public class ClubValidatorTests
    {
        [Test]
        public async Task Validate_InvalidClubCode_ShouldFail()
        {
            var options = CreateOptions();
            await DatabaseSeeder.SeedDatabase(options);

            using var context = new PathfinderContext(options);
            var validator = new ClubValidator(context);
            var clubDto = new Incoming.ClubDto
            {
                Name = "Test Club",
                ClubCode = "bad-code"
            };

            var result = await validator.TestValidateAsync(clubDto);
            result.ShouldHaveValidationErrorFor(c => c.ClubCode);
        }

        [Test]
        public async Task Validate_DuplicateClubCode_InPostRuleset_ShouldFail()
        {
            var options = CreateOptions();
            await DatabaseSeeder.SeedDatabase(options);

            using var context = new PathfinderContext(options);
            var validator = new ClubValidator(context);
            var clubDto = new Incoming.ClubDto
            {
                Name = "Duplicate Club",
                ClubCode = "VALIDCLUBCODE"
            };

            var result = await validator.TestValidateAsync(clubDto, opts => opts.IncludeRuleSets("post"));
            result.ShouldHaveValidationErrorFor(c => c.ClubCode);
        }

        [Test]
        public async Task Validate_ExcludeClubId_AllowsSameCode()
        {
            var options = CreateOptions();
            await DatabaseSeeder.SeedDatabase(options);

            using var context = new PathfinderContext(options);
            var existingClub = await context.Clubs.SingleAsync(c => c.ClubCode == "VALIDCLUBCODE");

            var validator = new ClubValidator(context);
            validator.SetExcludeClubId(existingClub.ClubID);

            var clubDto = new Incoming.ClubDto
            {
                Name = "Existing Club",
                ClubCode = "VALIDCLUBCODE"
            };

            var result = await validator.TestValidateAsync(clubDto, opts => opts.IncludeRuleSets("post"));
            result.ShouldNotHaveValidationErrorFor(c => c.ClubCode);
        }

        private static DbContextOptions<PathfinderContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase($"ClubValidatorTests-{Guid.NewGuid()}")
                .Options;
        }
    }
}
