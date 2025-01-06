using System;
using FluentValidation;
using System.Threading.Tasks;
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
    public class PathfinderHonorValidatorTests
    {
        public PathfinderHonorValidatorTests()
        {
            ContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        private PathfinderHonorValidator _pathfinderHonorValidator;

        protected DbContextOptions<PathfinderContext> ContextOptions { get; }

        [SetUp]
        public void SetUp()
        {
            var context = new PathfinderContext(ContextOptions);
            _pathfinderHonorValidator = new PathfinderHonorValidator(context);
            SeedDatabase(context);
        }

        [Test]
        public async Task Validate_InvalidHonorID_ValidationError()
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = 1
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                .WithSeverity(Severity.Error);
        }

        [Test]
        public async Task Validate_InvalidPathfinderID_ValidationError()
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = 1
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID)
                .WithSeverity(Severity.Error);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task Validate_InvalidStatusCode_ValidationError(int invalidStatusCode)
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = invalidStatusCode
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor, options =>
                {
                    options.IncludeAllRuleSets();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.StatusCode)
                .WithSeverity(Severity.Error);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task Validate_ValidStatusCode_ShouldPass(int statusCode)
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = statusCode
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor);

            validationResult.ShouldNotHaveValidationErrorFor(p => p.StatusCode);
        }

        [Test]
        public async Task Validate_DuplicateHonorAssignment_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                // Create and save test data
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options =>
                    {
                        options.IncludeRuleSets("post");
                    });

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithSeverity(Severity.Error);
            }
        }

        [Test]
        public async Task Validate_MultipleValidationRules_ValidationError()
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.Empty,
                PathfinderID = Guid.Empty,
                StatusCode = -1
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor, options =>
                {
                    options.IncludeRuleSets("post");
                    options.IncludeRulesNotInRuleSet();
                });

            validationResult.ShouldHaveValidationErrorFor(p => p.StatusCode);
            validationResult.ShouldHaveValidationErrorFor(p => p.HonorID);
            validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID);
        }

        [Test]
        public async Task Validate_StatusTransition_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                // Create initial honor with Planned status
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId,
                    StatusCode = 1, // Planned
                    Created = DateTime.UtcNow
                };
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                // Try to create duplicate with different status
                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 2 // Earned
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options =>
                    {
                        options.IncludeRuleSets("post");
                    });

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithSeverity(Severity.Error);
            }
        }

        [Test]
        public async Task Validate_NonexistentPathfinder_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var honorId = Guid.NewGuid();
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                await context.Honors.AddAsync(honor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = Guid.NewGuid(), // Non-existent pathfinder
                    StatusCode = 1,
                    Status = "Planned"
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options =>
                    {
                        options.IncludeRuleSets("post");
                    });

                validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID)
                    .WithSeverity(Severity.Error);
            }
        }

        [Test]
        public async Task Validate_DuplicateHonorAssignment_VerifyErrorMessage()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithErrorMessage($"Pathfinder {pathfinderId} already has honor {honorId}.");
            }
        }

        [Test]
        public async Task Validate_NonexistentHonor_VerifyErrorMessage()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                var pathfinder = new Pathfinder
                {
                    PathfinderID = pathfinderId,
                    Email = "test@test.com",
                    FirstName = "Test",
                    LastName = "User"
                };
                await context.Pathfinders.AddAsync(pathfinder);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithErrorMessage("Invalid Honor ID provided.");
            }
        }

        [Test]
        public async Task Validate_NonexistentPathfinder_VerifyErrorMessage()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                await context.Honors.AddAsync(honor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID)
                    .WithErrorMessage($"Invalid Pathfinder ID {pathfinderId} provided.");
            }
        }

        [Test]
        public async Task Validate_InvalidStatusCode_VerifyErrorMessage()
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = 0,
                Status = "Invalid"
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor);

            validationResult.ShouldHaveValidationErrorFor(p => p.StatusCode)
                .WithErrorMessage($"Honor status {newPathfinderHonor.Status} is invalid. Valid statuses are: Planned, Earned, Awarded.");
        }

        [Test]
        public async Task Validate_ExistingHonorDifferentPathfinder_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                // Create test data
                var pathfinderId1 = Guid.NewGuid();
                var pathfinderId2 = Guid.NewGuid();
                var honorId = Guid.NewGuid();

                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };

                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId1,
                    HonorID = honorId,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };

                await context.Honors.AddAsync(honor);
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId2,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveValidationErrorFor(p => p.HonorID);
            }
        }

        [Test]
        public async Task Validate_NonexistentHonorAndPathfinder_MultipleErrors()
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = 1
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

            validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                .WithErrorMessage("Invalid Honor ID provided.");
            validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID)
                .WithErrorMessage($"Invalid Pathfinder ID {newPathfinderHonor.PathfinderID} provided.");
        }

        [Test]
        public async Task Validate_DuplicateHonorAssignment_ExactErrorMessage()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };

                await context.Honors.AddAsync(honor);
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithErrorMessage($"Pathfinder {pathfinderId} already has honor {honorId}.");
            }
        }

        [Test]
        public async Task Validate_InvalidStatusCode_ExactErrorMessage()
        {
            var newPathfinderHonor = new Incoming.PathfinderHonorDto
            {
                HonorID = Guid.NewGuid(),
                PathfinderID = Guid.NewGuid(),
                StatusCode = 0,
                Status = "Invalid"
            };

            var validationResult = await _pathfinderHonorValidator
                .TestValidateAsync(newPathfinderHonor);

            validationResult.ShouldHaveValidationErrorFor(p => p.StatusCode)
                .WithErrorMessage($"Honor status {newPathfinderHonor.Status} is invalid. Valid statuses are: Planned, Earned, Awarded.");
        }

        [Test]
        public async Task Validate_EmptyGuidPathfinder_ValidationError()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var honorId = Guid.NewGuid();
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                await context.Honors.AddAsync(honor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = Guid.Empty,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID)
                    .WithErrorMessage($"Invalid Pathfinder ID {Guid.Empty} provided.");
            }
        }

        [Test]
        public async Task Validate_SameHonorDifferentPathfinder_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId1 = Guid.NewGuid();
                var pathfinderId2 = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                
                var pathfinder1 = new Pathfinder
                {
                    PathfinderID = pathfinderId1,
                    Email = "test1@test.com",
                    FirstName = "Test 1",
                    LastName = "User 1"
                };
                var pathfinder2 = new Pathfinder
                {
                    PathfinderID = pathfinderId2,
                    Email = "test2@test.com",
                    FirstName = "Test 2",
                    LastName = "User 2"
                };
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId1,
                    HonorID = honorId,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };

                await context.Pathfinders.AddRangeAsync(pathfinder1, pathfinder2);
                await context.Honors.AddAsync(honor);
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId2,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveAnyValidationErrors();
            }
        }

        [Test]
        public async Task Validate_DifferentHonorSamePathfinder_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId1 = Guid.NewGuid();
                var honorId2 = Guid.NewGuid();
                
                var pathfinder = new Pathfinder
                {
                    PathfinderID = pathfinderId,
                    Email = "test@test.com",
                    FirstName = "Test",
                    LastName = "User"
                };
                var honor1 = new Honor
                {
                    HonorID = honorId1,
                    Name = "Test Honor 1",
                    Level = 1,
                    PatchFilename = "test1.jpg",
                    WikiPath = new Uri("https://example.com/1")
                };
                var honor2 = new Honor
                {
                    HonorID = honorId2,
                    Name = "Test Honor 2",
                    Level = 1,
                    PatchFilename = "test2.jpg",
                    WikiPath = new Uri("https://example.com/2")
                };
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId1,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };

                await context.Pathfinders.AddAsync(pathfinder);
                await context.Honors.AddRangeAsync(honor1, honor2);
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId2,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveAnyValidationErrors();
            }
        }

        [Test]
        public async Task Validate_SameHonorSamePathfinder_ShouldFail()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                
                var pathfinder = new Pathfinder
                {
                    PathfinderID = pathfinderId,
                    Email = "test@test.com",
                    FirstName = "Test",
                    LastName = "User"
                };
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };

                await context.Pathfinders.AddAsync(pathfinder);
                await context.Honors.AddAsync(honor);
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithErrorMessage($"Pathfinder {pathfinderId} already has honor {honorId}.");
            }
        }

        [Test]
        public async Task Validate_HonorExistsButPathfinderDoesNot_ShouldFail()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };

                await context.Honors.AddAsync(honor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.PathfinderID)
                    .WithErrorMessage($"Invalid Pathfinder ID {pathfinderId} provided.");
                validationResult.ShouldNotHaveValidationErrorFor(p => p.HonorID);
            }
        }

        [Test]
        public async Task Validate_PathfinderExistsButHonorDoesNot_ShouldFail()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                
                var pathfinder = new Pathfinder
                {
                    PathfinderID = pathfinderId,
                    Email = "test@test.com",
                    FirstName = "Test",
                    LastName = "User"
                };

                await context.Pathfinders.AddAsync(pathfinder);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithErrorMessage("Invalid Honor ID provided.");
                validationResult.ShouldNotHaveValidationErrorFor(p => p.PathfinderID);
            }
        }

        [Test]
        public async Task Validate_SameHonorIdDifferentPathfinderId_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId1 = Guid.NewGuid();
                var pathfinderId2 = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId1,
                    HonorID = honorId,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };

                await context.Honors.AddAsync(honor);
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId2,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveValidationErrorFor(p => p.HonorID);
            }
        }

        [Test]
        public async Task Validate_DifferentHonorIdSamePathfinderId_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId1 = Guid.NewGuid();
                var honorId2 = Guid.NewGuid();
                
                var honor1 = new Honor
                {
                    HonorID = honorId1,
                    Name = "Test Honor 1",
                    Level = 1,
                    PatchFilename = "test1.jpg",
                    WikiPath = new Uri("https://example.com/1")
                };
                var honor2 = new Honor
                {
                    HonorID = honorId2,
                    Name = "Test Honor 2",
                    Level = 1,
                    PatchFilename = "test2.jpg",
                    WikiPath = new Uri("https://example.com/2")
                };
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId1,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };

                await context.Honors.AddRangeAsync(honor1, honor2);
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId2,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveValidationErrorFor(p => p.HonorID);
            }
        }

        [Test]
        public async Task Validate_HonorExistsExactMatch_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var honorId = Guid.NewGuid();
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };

                await context.Honors.AddAsync(honor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = Guid.NewGuid(),
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveValidationErrorFor(p => p.HonorID);
            }
        }

        [Test]
        public async Task Validate_HonorDoesNotExist_ShouldFail()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var honorId = Guid.NewGuid();
                var differentHonorId = Guid.NewGuid();
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };

                await context.Honors.AddAsync(honor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = differentHonorId,
                    PathfinderID = Guid.NewGuid(),
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithErrorMessage("Invalid Honor ID provided.");
            }
        }

        [Test]
        public async Task Validate_SameHonorIdSamePathfinderId_ShouldFail()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };

                await context.Honors.AddAsync(honor);
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithErrorMessage($"Pathfinder {pathfinderId} already has honor {honorId}.");
            }
        }

        [Test]
        public async Task Validate_SameHonorDifferentPathfinderWithExistingPathfinder_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId1 = Guid.NewGuid();
                var pathfinderId2 = Guid.NewGuid();
                var honorId = Guid.NewGuid();
                
                var pathfinder1 = new Pathfinder
                {
                    PathfinderID = pathfinderId1,
                    Email = "test1@test.com",
                    FirstName = "Test 1",
                    LastName = "User 1"
                };
                var pathfinder2 = new Pathfinder
                {
                    PathfinderID = pathfinderId2,
                    Email = "test2@test.com",
                    FirstName = "Test 2",
                    LastName = "User 2"
                };
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId1,
                    HonorID = honorId,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };

                await context.Pathfinders.AddRangeAsync(pathfinder1, pathfinder2);
                await context.Honors.AddAsync(honor);
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId2,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveValidationErrorFor(p => p.HonorID);
                validationResult.ShouldNotHaveValidationErrorFor(p => p.PathfinderID);
            }
        }

        [Test]
        public async Task Validate_DifferentHonorSamePathfinderWithExistingHonor_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var pathfinderId = Guid.NewGuid();
                var honorId1 = Guid.NewGuid();
                var honorId2 = Guid.NewGuid();
                
                var pathfinder = new Pathfinder
                {
                    PathfinderID = pathfinderId,
                    Email = "test@test.com",
                    FirstName = "Test",
                    LastName = "User"
                };
                var honor1 = new Honor
                {
                    HonorID = honorId1,
                    Name = "Test Honor 1",
                    Level = 1,
                    PatchFilename = "test1.jpg",
                    WikiPath = new Uri("https://example.com/1")
                };
                var honor2 = new Honor
                {
                    HonorID = honorId2,
                    Name = "Test Honor 2",
                    Level = 1,
                    PatchFilename = "test2.jpg",
                    WikiPath = new Uri("https://example.com/2")
                };
                var existingHonor = new PathfinderHonor
                {
                    PathfinderID = pathfinderId,
                    HonorID = honorId1,
                    StatusCode = 1,
                    Created = DateTime.UtcNow
                };

                await context.Pathfinders.AddAsync(pathfinder);
                await context.Honors.AddRangeAsync(honor1, honor2);
                await context.PathfinderHonors.AddAsync(existingHonor);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId2,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveValidationErrorFor(p => p.HonorID);
                validationResult.ShouldNotHaveValidationErrorFor(p => p.PathfinderID);
            }
        }

        [Test]
        public async Task Validate_HonorExistsWithExactMatch_ShouldPass()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var honorId = Guid.NewGuid();
                var pathfinderId = Guid.NewGuid();
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                var pathfinder = new Pathfinder
                {
                    PathfinderID = pathfinderId,
                    Email = "test@test.com",
                    FirstName = "Test",
                    LastName = "User"
                };

                await context.Honors.AddAsync(honor);
                await context.Pathfinders.AddAsync(pathfinder);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = honorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldNotHaveValidationErrorFor(p => p.HonorID);
                validationResult.ShouldNotHaveValidationErrorFor(p => p.PathfinderID);
            }
        }

        [Test]
        public async Task Validate_HonorExistsWithDifferentId_ShouldFail()
        {
            using (var context = new PathfinderContext(ContextOptions))
            {
                var honorId = Guid.NewGuid();
                var differentHonorId = Guid.NewGuid();
                var pathfinderId = Guid.NewGuid();
                var honor = new Honor
                {
                    HonorID = honorId,
                    Name = "Test Honor",
                    Level = 1,
                    PatchFilename = "test.jpg",
                    WikiPath = new Uri("https://example.com")
                };
                var pathfinder = new Pathfinder
                {
                    PathfinderID = pathfinderId,
                    Email = "test@test.com",
                    FirstName = "Test",
                    LastName = "User"
                };

                await context.Honors.AddAsync(honor);
                await context.Pathfinders.AddAsync(pathfinder);
                await context.SaveChangesAsync();

                var newPathfinderHonor = new Incoming.PathfinderHonorDto
                {
                    HonorID = differentHonorId,
                    PathfinderID = pathfinderId,
                    StatusCode = 1
                };

                var validationResult = await _pathfinderHonorValidator
                    .TestValidateAsync(newPathfinderHonor, options => options.IncludeRuleSets("post"));

                validationResult.ShouldHaveValidationErrorFor(p => p.HonorID)
                    .WithErrorMessage("Invalid Honor ID provided.");
            }
        }

        public static void SeedDatabase(PathfinderContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var validPathfinderID = Guid.NewGuid();
            var validHonorID = Guid.NewGuid();

            context.Pathfinders.Add(
                new Pathfinder
                {
                    PathfinderID = validPathfinderID,
                    FirstName = "test1",
                    LastName = "pathfinder",
                    Email = "test1@validemail.com",
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                });

            context.Honors.Add(
                new Honor
                {
                    HonorID = validHonorID
                });

            context.SaveChangesAsync();
        }
    }
}
