using NUnit.Framework;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

//using PathfinderHonorManager.Tests.DataFixtures;
using PathfinderHonorManager.DataAccess;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using PathfinderHonorManager.Validators;
using PathfinderHonorManager.Model;
using System.Threading.Tasks;
using AutoMapper;


using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.Sqlite;

namespace PathfinderHonorManager.Tests
{
    [TestFixture]
    public abstract class PathfinderValidatorTests
    {
        protected PathfinderValidatorTests(DbContextOptions<PathfinderContext> contextOptions)
        {
            ContextOptions = contextOptions;
        }

        private PathfinderValidator _pathfinderValidator;

        protected DbContextOptions<PathfinderContext> ContextOptions { get; }

        private readonly IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            using var context = new PathfinderContext(ContextOptions);
            _pathfinderValidator = new PathfinderValidator(context);
            //AddPathfinders(context);
        }

        // Email tests
        [TestCase("")]
        [TestCase(null)]
        [TestCase("randomnonemailstring")]
        public void Validate_InvalidEmail_ValidationError(string email)
        {
            using var context = new PathfinderContext(ContextOptions);
            var newPathfinder = new Incoming.PathfinderDto
            {
                FirstName = "test",
                LastName = "user",
                Email = email
            };

            var validationResult = _pathfinderValidator
                .TestValidate(newPathfinder);

            validationResult.ShouldHaveValidationErrorFor(p => p.Email)
                .WithSeverity(Severity.Error);

        }

        // FirstName tests
        [TestCase("")]
        [TestCase(null)]
        public void Validate_FirstName_ValidationError(string firstName)
        {
            using var context = new PathfinderContext(ContextOptions);
            var newPathfinder = new Incoming.PathfinderDto
            {
                FirstName = firstName,
                LastName = "user",
                Email = "test@email.com"
            };

            var validationResult = _pathfinderValidator
                .TestValidate(newPathfinder);

            validationResult.ShouldHaveValidationErrorFor(p => p.FirstName)
                .WithSeverity(Severity.Error);

        }

        // LastName tests
        [TestCase("")]
        [TestCase(null)]
        public void Validate_LastName_ValidationError(string lastName)
        {
            using var context = new PathfinderContext(ContextOptions);
            var newPathfinder = new Incoming.PathfinderDto
            {
                FirstName = "test",
                LastName = lastName,
                Email = "test@email.com"
            };

            var validationResult = _pathfinderValidator
                .TestValidate(newPathfinder);

            validationResult.ShouldHaveValidationErrorFor(p => p.LastName)
                .WithSeverity(Severity.Error);

        }

        private void AddPathfinders(PathfinderContext context)
        {
            context.Pathfinders.AddRange(
                new Pathfinder[]
                {
                    new Pathfinder
                        {
                            FirstName = "test1",
                            LastName = "pathfinder",
                            Email = "test@validemail.com"
                        },

                    new Pathfinder
                        {
                            FirstName = "test1",
                            LastName = "pathfinder",
                            Email = "test@validemail.com"
                        },
                    new Pathfinder
                        {
                            FirstName = "test2",
                            LastName = "pathfinder",
                            Email = "test2@validemail.com"
                        },
                    new Pathfinder
                        {
                            FirstName = "test3",
                            LastName = "pathfinder",
                            Email = "test3@validemail.com"
                        }
                });

            context.SaveChanges();
        }
    }
}
   
