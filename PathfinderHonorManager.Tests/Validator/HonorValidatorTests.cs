using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Validators;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Tests.Validator
{
    [TestFixture]
    public class HonorValidatorTests
    {
        private HonorValidator _honorValidator;
        protected DbContextOptions<PathfinderContext> ContextOptions { get; }

        public HonorValidatorTests()
        {
            ContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        [SetUp]
        public void SetUp()
        {
            var context = new PathfinderContext(ContextOptions);
            _honorValidator = new HonorValidator(context);
        }

        [TestCase("test.txt")]
        [TestCase("test")]
        public async Task Validate_InvalidFileExtension_ShouldFail(string filename)
        {
            var honorDto = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 2,
                PatchFilename = filename,
                WikiPath = new Uri("https://example.com")
            };

            var result = await _honorValidator.TestValidateAsync(honorDto);
            result.ShouldHaveValidationErrorFor(x => x.PatchFilename);
        }

        [TestCase("ftp://example.com")]
        [TestCase("file://example.com")]
        public async Task Validate_NonHttpUri_ShouldFail(string url)
        {
            var honorDto = new Incoming.HonorDto
            {
                Name = "Test Honor",
                Level = 2,
                PatchFilename = "test.png",
                WikiPath = new Uri(url)
            };

            var result = await _honorValidator.TestValidateAsync(honorDto);
            result.ShouldHaveValidationErrorFor(x => x.WikiPath);
        }

        [Test]
        public async Task Validate_DuplicateName_InPostRuleset_ShouldFail()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                await context.Honors.AddAsync(new Honor
                {
                    Name = "Existing Honor",
                    Level = 1,
                    PatchFilename = "test.png",
                    WikiPath = new Uri("https://example.com")
                });
                await context.SaveChangesAsync();

                var validator = new HonorValidator(context);
                var honorDto = new Incoming.HonorDto
                {
                    Name = "Existing Honor",
                    Level = 2,
                    PatchFilename = "test.png",
                    WikiPath = new Uri("https://example.com")
                };

                var result = await validator.TestValidateAsync(honorDto, opt => 
                    opt.IncludeRuleSets("post"));

                result.ShouldHaveValidationErrorFor(x => x.Name);
            }
        }
    }
}
