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
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.Sqlite;
using System;

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
        private DbContextOptions<PathfinderContext> options;

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
        [TestCase("nonemailstring")]
        public async Task Validate_InvalidEmail_ValidationError(string email)
        {
            //using var context = new PathfinderContext(ContextOptions);
            var newPathfinder = new Incoming.PathfinderDto
            {
                FirstName = "test",
                LastName = "user",
                Email = email
            };

            var validationResult = await _pathfinderValidator
                .TestValidateAsync(newPathfinder);

            validationResult.ShouldHaveValidationErrorFor(p => p.Email)
                .WithSeverity(Severity.Error);

        }

        // FirstName tests
        [TestCase("")]
        [TestCase(null)]
        public async Task Validate_FirstName_ValidationError(string firstName)
        {
            //using var context = new PathfinderContext(ContextOptions);
            var randEmail = RandomString(10);
            var newPathfinder = new Incoming.PathfinderDto
            {
                FirstName = firstName,
                LastName = "user",
                //Email = $"{randEmail}@email.com"
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
            //using var context = new PathfinderContext(ContextOptions);
            var newPathfinder = new Incoming.PathfinderDto
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

        public static void AddPathfinders(PathfinderContext context)
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

            context.SaveChanges();
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
   
