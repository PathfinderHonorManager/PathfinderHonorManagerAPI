using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Tests.Helpers;

namespace PathfinderHonorManager.Tests.Service
{
    public class ClubServiceTests
    {
        public ClubServiceTests()
        {
            ContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        protected DbContextOptions<PathfinderContext> ContextOptions { get; }

        private ClubService _clubService;

        private List<Club> _clubs;

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
            _clubs = new List<Club>
        {
            new Club
            {
                ClubID = Guid.NewGuid(),
                Name = "Test Club 1"
            },
            new Club
            {
                ClubID = Guid.NewGuid(),
                Name = "Test Club 2"
            },
            new Club
            {
                ClubID = Guid.NewGuid(),
                Name = "Test Club 3"
            }
    };


            await dbContext.Clubs.AddRangeAsync(_clubs);
            await dbContext.SaveChangesAsync();
        }

        [TestCase]
        public async Task GetAllAsync_ReturnsAllClubs()
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();

            var logger = new NullLogger<ClubService>();

            var validator = new DummyValidator<ClubDto>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _clubService = new ClubService(dbContext, mapper, logger);

                // Act
                CancellationToken token = new();
                var result = await _clubService.GetAllAsync(token);
                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(_clubs.Count, result.Count);
            }
        }

        // ...

        [TestCase(0)]
        public async Task GetByIdAsync_ClubExists_ReturnsClub(int clubIndex)
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            var logger = new NullLogger<ClubService>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _clubService = new ClubService(dbContext, mapper, logger);

                // Act
                CancellationToken token = new();
                var clubId = _clubs[clubIndex].ClubID;
                var result = await _clubService.GetByIdAsync(clubId, token);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(clubId, result.ClubID);
                Assert.AreEqual(_clubs[clubIndex].Name, result.Name);
            }
        }

        [TestCase]
        public async Task GetByIdAsync_ClubDoesNotExist_ReturnsNull()
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            var logger = new NullLogger<ClubService>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _clubService = new ClubService(dbContext, mapper, logger);

                // Act
                CancellationToken token = new();
                var clubId = Guid.NewGuid();
                var result = await _clubService.GetByIdAsync(clubId, token);

                // Assert
                Assert.IsNull(result);
            }
        }


        [TearDown]
        public async Task TearDown()
        {
            using var dbContext = new PathfinderContext(ContextOptions);

            dbContext.Clubs.RemoveRange();

            await dbContext.SaveChangesAsync();
        }
    }

}
