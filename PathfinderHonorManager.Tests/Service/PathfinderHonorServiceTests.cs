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

public class PathfinderHonorServiceTests
{
    public PathfinderHonorServiceTests()
    {
        ContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
    }

    protected DbContextOptions<PathfinderContext> ContextOptions { get; }

    private DbContextOptions<PathfinderContext> _dbContextOptions;
    private PathfinderContext _dbContext;
    private PathfinderHonorService _pathfinderHonorService;

    private Pathfinder _pathfinder;
    private Honor _honor;
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
        _pathfinder = new Pathfinder
        {
            PathfinderID = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "johndoe@example.com",
            Grade = 1,
            ClubID = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };

        _honor = new Honor
        {
            HonorID = Guid.NewGuid(),
            Name = "Test Honor",
            Level = 1,
            Description = "Test description",
            PatchFilename = "test_patch.jpg",
            WikiPath = new Uri("https://example.com/test")
        };
        _pathfinderHonors = new List<PathfinderHonor>
        {
        new PathfinderHonor
        {
            PathfinderHonorID = Guid.NewGuid(),
            HonorID = _honor.HonorID,
            StatusCode = (int)HonorStatus.Planned,
            Created = DateTime.UtcNow,
            PathfinderID = _pathfinder.PathfinderID
        },
        new PathfinderHonor
        {
            PathfinderHonorID = Guid.NewGuid(),
            HonorID = _honor.HonorID,
            StatusCode = (int)HonorStatus.Earned,
            Created = DateTime.UtcNow,
            PathfinderID = _pathfinder.PathfinderID
        },
        new PathfinderHonor
        {
            PathfinderHonorID = Guid.NewGuid(),
            HonorID = _honor.HonorID,
            StatusCode = (int)HonorStatus.Awarded,
            Created = DateTime.UtcNow,
            PathfinderID = _pathfinder.PathfinderID
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
        await dbContext.Pathfinders.AddAsync(_pathfinder);
        await dbContext.Honors.AddAsync(_honor);
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

    [TearDown]
    public async Task TearDown()
    {
        using var dbContext = new PathfinderContext(ContextOptions);

        dbContext.PathfinderHonors.RemoveRange(_pathfinderHonors);
        dbContext.Honors.Remove(_honor);
        dbContext.Pathfinders.Remove(_pathfinder);
        dbContext.PathfinderHonorStatuses.RemoveRange(dbContext.PathfinderHonorStatuses);

        await dbContext.SaveChangesAsync();
    }
}
