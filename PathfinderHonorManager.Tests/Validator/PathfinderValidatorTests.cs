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
    public class PathfinderValidatorTests
    {
        public PathfinderValidatorTests()
        {
            ContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        private PathfinderValidator _pathfinderValidator;
        
        protected DbContextOptions<PathfinderContext> ContextOptions { get; }

        [SetUp]
        public void SetUp()
        {
            var context = new PathfinderContext(ContextOptions);
            _pathfinderValidator = new PathfinderValidator(context);
            SeedDatabase(context);
        }

        // Email tests
        [TestCase("")]
        [TestCase(null)]
        [TestCase("nonemailstring")]
        public async Task Validate_InvalidEmail_ValidationError(string email)
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "test",
                LastName = "user",
                Email = email
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.Email)
                .WithSeverity(Severity.Error);

        }

        // FirstName tests
        [TestCase("")]
        [TestCase(null)]
        public async Task Validate_FirstName_ValidationError(string firstName)
        {
            var randEmail = RandomString(10);
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = firstName,
                LastName = "user",
                Email = "test@email.com"
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder);

            validationResult.ShouldHaveValidationErrorFor(p => p.FirstName)
                .WithSeverity(Severity.Error);

        }

        // LastName tests
        [TestCase("")]
        [TestCase(null)]
        public async Task Validate_LastName_ValidationError(string lastName)
        {
            var newPathfinder = new Incoming.PathfinderDtoInternal
            {
                FirstName = "test",
                LastName = lastName,
                Email = "test@email.com"
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder);

            validationResult.ShouldHaveValidationErrorFor(p => p.LastName)
                .WithSeverity(Severity.Error);
        }

        public static void SeedDatabase(PathfinderContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Pathfinders.AddRange(
                new Pathfinder[]
                {

                    new Pathfinder
                        {
                            FirstName = "test1",
                            LastName = "pathfinder",
                            Email = "test1@validemail.com",
                            Created = System.DateTime.Now,
                            Updated = System.DateTime.Now
                        },
                    new Pathfinder
                        {
                            FirstName = "test2",
                            LastName = "pathfinder",
                            Email = "test2@validemail.com",
                            Created = System.DateTime.Now,
                            Updated = System.DateTime.Now
                        },
                    new Pathfinder
                        {
                            FirstName = "test3",
                            LastName = "pathfinder",
                            Email = "test3@validemail.com",
                            Created = System.DateTime.Now,
                            Updated = System.DateTime.Now
                        }
                }); ;

            context.SaveChangesAsync();
        }
        public static string RandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);

            return finalString;
        }
    }
}
   
