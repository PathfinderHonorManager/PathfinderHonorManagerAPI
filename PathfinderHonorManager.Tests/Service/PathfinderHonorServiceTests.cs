using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Incoming;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Model.Enum;
using PathfinderHonorManager.Service;

namespace PathfinderHonorManager.Tests.Service
{
    public class PathfinderHonorServiceTests
    {
        public PathfinderHonorServiceTests()
        {
            ContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        protected DbContextOptions<PathfinderContext> ContextOptions { get; }

        private PathfinderHonorService _pathfinderHonorService;

        private List<Pathfinder> _pathfinders;
        private List<Honor> _honors;
        private List<PathfinderHonor> _pathfinderHonors;

        [SetUp]
        public async Task SetUp()
        {
            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                await SeedDatabase(dbContext);
            }
        }

        private async Task SeedDatabase(PathfinderContext dbContext)
        {
            _pathfinders = new List<Pathfinder>
        {
            new Pathfinder
            {
                PathfinderID = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@example.com",
                Grade = 1,
                ClubID = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            },
                new Pathfinder
            {
                PathfinderID = Guid.NewGuid(),
                FirstName = "Addy",
                LastName = "Addsome",
                Email = "addyaddsome@example.com",
                Grade = 1,
                ClubID = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            }
    };


            _honors = new List<Honor>
    {
        new Honor
        {
            HonorID = Guid.NewGuid(),
            Name = "Test Honor",
            Level = 1,
            Description = "Test description",
            PatchFilename = "test_patch.jpg",
            WikiPath = new Uri("https://example.com/test")
        },
        new Honor
        {
            HonorID = Guid.NewGuid(),
            Name = "Test Honor 2",
            Level = 1,
            Description = "Test description 2",
            PatchFilename = "test_patch2.jpg",
            WikiPath = new Uri("https://example.com/test2")
        },
        new Honor
        {
            HonorID = Guid.NewGuid(),
            Name = "Test Honor 3",
            Level = 1,
            Description = "Test description 3",
            PatchFilename = "test_patch3.jpg",
            WikiPath = new Uri("https://example.com/test3")
        }
    };

            _pathfinderHonors = new List<PathfinderHonor>
    {
        new PathfinderHonor
        {
            PathfinderHonorID = Guid.NewGuid(),
            HonorID = _honors[0].HonorID,
            StatusCode = (int)HonorStatus.Planned,
            Created = DateTime.UtcNow,
            PathfinderID = _pathfinders[0].PathfinderID
        },
        new PathfinderHonor
        {
            PathfinderHonorID = Guid.NewGuid(),
            HonorID = _honors[1].HonorID,
            StatusCode = (int)HonorStatus.Earned,
            Created = DateTime.UtcNow,
            PathfinderID = _pathfinders[0].PathfinderID
        },
        new PathfinderHonor
        {
            PathfinderHonorID = Guid.NewGuid(),
            HonorID = _honors[2].HonorID,
            StatusCode = (int)HonorStatus.Awarded,
            Created = DateTime.UtcNow,
            PathfinderID = _pathfinders[0].PathfinderID
        }
    };
            var pathfinderHonorStatuses = new List<PathfinderHonorStatus>
        {
            new PathfinderHonorStatus
            {
                StatusCode = (int)HonorStatus.Planned,
                Status = HonorStatus.Planned.ToString()
            },
            new PathfinderHonorStatus
            {
                StatusCode = (int)HonorStatus.Earned,
                Status = HonorStatus.Earned.ToString()
            },
            new PathfinderHonorStatus
            {
                StatusCode = (int)HonorStatus.Awarded,
                Status = HonorStatus.Awarded.ToString()
            }
        };

            await dbContext.PathfinderHonorStatuses.AddRangeAsync(pathfinderHonorStatuses);
            await dbContext.Pathfinders.AddRangeAsync(_pathfinders);
            await dbContext.Honors.AddRangeAsync(_honors);
            await dbContext.PathfinderHonors.AddRangeAsync(_pathfinderHonors);
            await dbContext.SaveChangesAsync();
        }

        [Test]
        [TestCase("planned")]
        [TestCase("earned")]
        [TestCase("awarded")]
        public async Task GetAllByStatusAsync_ReturnsPathfinderHonorsForStatus(string status)
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();

            var logger = new NullLogger<PathfinderHonorService>();

            var validator = new DummyValidator<PathfinderHonorDto>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _pathfinderHonorService = new PathfinderHonorService(dbContext, mapper, validator, logger);

                // Act
                CancellationToken token = new();
                var result = await _pathfinderHonorService.GetAllByStatusAsync(status, token);
                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                Assert.IsTrue(result.All(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase)));
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task GetByIdAsync_ReturnsPathfinderHonorsForPathfinderIdAndHonorId(int id)
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();

            var logger = new NullLogger<PathfinderHonorService>();

            var validator = new DummyValidator<PathfinderHonorDto>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _pathfinderHonorService = new PathfinderHonorService(dbContext, mapper, validator, logger);

                // Act
                CancellationToken token = new();
                var result = await _pathfinderHonorService.GetByIdAsync(_pathfinders[0].PathfinderID, _pathfinderHonors[id].HonorID, token);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(_pathfinders[0].PathfinderID, result.PathfinderID);
                Assert.AreEqual(_pathfinderHonors[id].HonorID, result.HonorID);
            }
        }

        [TestCase(0, "earned")]
        [TestCase(1, "planned")]
        [TestCase(2, "awarded")]
        public async Task AddAsync_AddsNewPathfinderHonorAndReturnsDto(int honorIndex, string honorStatus)
        {
            // Arrange
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();

            var logger = new NullLogger<PathfinderHonorService>();

            var validator = new DummyValidator<PathfinderHonorDto>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _pathfinderHonorService = new PathfinderHonorService(dbContext, mapper, validator, logger);

                var postPathfinderHonorDto = new PostPathfinderHonorDto
                {
                    HonorID = _honors[honorIndex].HonorID,
                    Status = honorStatus.ToString()
                };
                CancellationToken token = new();

                // Act
                var result = await _pathfinderHonorService.AddAsync(_pathfinders[1].PathfinderID, postPathfinderHonorDto, token);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(_pathfinders[1].PathfinderID, result.PathfinderID);
                Assert.AreEqual(_honors[honorIndex].HonorID, result.HonorID);
                Assert.IsTrue(result.Status.Equals(honorStatus, StringComparison.OrdinalIgnoreCase));
            }
        }

        [TestCase(0, 0, "awarded")]
        [TestCase(1, 1, "earned")]
        [TestCase(2, 2, "planned")]
        public async Task UpdateAsync_UpdatesPathfinderHonorAndReturnsUpdatedDto(int honorIndex, int pathfinderHonorIndex, string honorStatus)
        {
            // Arrange
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();

            var logger = new NullLogger<PathfinderHonorService>();

            var validator = new DummyValidator<PathfinderHonorDto>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _pathfinderHonorService = new PathfinderHonorService(dbContext, mapper, validator, logger);

                var putPathfinderHonorDto = new PutPathfinderHonorDto
                {
                    Status = honorStatus.ToString()
                };
                CancellationToken token = new();

                // Act
                var result = await _pathfinderHonorService.UpdateAsync(_pathfinders[0].PathfinderID, _honors[honorIndex].HonorID, putPathfinderHonorDto, token);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(_pathfinders[0].PathfinderID, result.PathfinderID);
                Assert.AreEqual(_honors[honorIndex].HonorID, result.HonorID);
                Assert.IsTrue(result.Status.Equals(honorStatus, StringComparison.OrdinalIgnoreCase));
            }
        }

        private class DummyValidator<T> : AbstractValidator<T>
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

        [TearDown]
        public async Task TearDown()
        {
            using var dbContext = new PathfinderContext(ContextOptions);

            dbContext.PathfinderHonors.RemoveRange(_pathfinderHonors);
            dbContext.Honors.RemoveRange(_honors);
            dbContext.Pathfinders.RemoveRange(_pathfinders);
            dbContext.PathfinderHonorStatuses.RemoveRange(dbContext.PathfinderHonorStatuses);

            await dbContext.SaveChangesAsync();
        }
    }

}
