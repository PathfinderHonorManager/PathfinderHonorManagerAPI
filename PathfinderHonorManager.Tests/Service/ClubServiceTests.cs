using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Tests.Helpers;
using PathfinderHonorManager.Validators;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Tests.Service
{
    public class ClubServiceTests
    {
        protected DbContextOptions<PathfinderContext> ContextOptions { get; }
        private ClubService _clubService;
        private List<Club> _clubs;
        private IValidator<Incoming.ClubDto> _validator;

        public ClubServiceTests()
        {
            ContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        [SetUp]
        public async Task SetUp()
        {
            using var dbContext = new PathfinderContext(ContextOptions);
            await DatabaseCleaner.CleanDatabase(dbContext);
            await SeedDatabase(dbContext);
        }

        private async Task SeedDatabase(PathfinderContext dbContext)
        {
            _clubs = new List<Club>
            {
                new Club
                {
                    ClubID = Guid.NewGuid(),
                    ClubCode = "TESTCLUB1",
                    Name = "Test Club 1"
                },
                new Club
                {
                    ClubID = Guid.NewGuid(),
                    ClubCode = "TESTCLUB2",
                    Name = "Test Club 2"
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

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _validator = new ClubValidator(dbContext);
                _clubService = new ClubService(dbContext, mapper, logger, _validator);

                // Act
                CancellationToken token = new();
                var result = await _clubService.GetAllAsync(token);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count, Is.EqualTo(_clubs.Count));
            }
        }

        [TestCase(0)]
        public async Task GetByIdAsync_ClubExists_ReturnsClub(int clubIndex)
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            var logger = new NullLogger<ClubService>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _validator = new ClubValidator(dbContext);
                _clubService = new ClubService(dbContext, mapper, logger, _validator);

                // Act
                CancellationToken token = new();
                var clubId = _clubs[clubIndex].ClubID;
                var result = await _clubService.GetByIdAsync(clubId, token);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.ClubID, Is.EqualTo(clubId));
                Assert.That(result.Name, Is.EqualTo(_clubs[clubIndex].Name));
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
                _validator = new ClubValidator(dbContext);
                _clubService = new ClubService(dbContext, mapper, logger, _validator);

                // Act
                CancellationToken token = new();
                var clubId = Guid.NewGuid();
                var result = await _clubService.GetByIdAsync(clubId, token);

                // Assert
                Assert.That(result, Is.Null);
            }
        }
        
        [TestCase]
        public async Task CreateAsync_ValidClub_ReturnsCreatedClub()
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            var logger = new NullLogger<ClubService>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _validator = new ClubValidator(dbContext);
                _clubService = new ClubService(dbContext, mapper, logger, _validator);

                // Act
                CancellationToken token = new();
                var newClub = new Incoming.ClubDto
                {
                    Name = "New Test Club",
                    ClubCode = "NEWCLUB"
                };

                var result = await _clubService.CreateAsync(newClub, token);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Name, Is.EqualTo(newClub.Name));
                Assert.That(result.ClubCode, Is.EqualTo(newClub.ClubCode));
                Assert.That(result.ClubID, Is.Not.EqualTo(Guid.Empty));

                // Verify the club was added to the database
                var savedClub = await dbContext.Clubs.SingleOrDefaultAsync(c => c.ClubCode == newClub.ClubCode);
                Assert.That(savedClub, Is.Not.Null);
                Assert.That(savedClub.Name, Is.EqualTo(newClub.Name));
            }
        }

        [TestCase]
        public async Task CreateAsync_DuplicateClubCode_ThrowsValidationException()
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            var logger = new NullLogger<ClubService>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _validator = new ClubValidator(dbContext);
                _clubService = new ClubService(dbContext, mapper, logger, _validator);

                // Act
                CancellationToken token = new();
                var newClub = new Incoming.ClubDto
                {
                    Name = "Duplicate Club",
                    ClubCode = _clubs[0].ClubCode // Using existing club code to trigger exception
                };

                // Assert
                Assert.ThrowsAsync<ValidationException>(() => 
                    _clubService.CreateAsync(newClub, token));
            }
        }

        [TestCase(0)]
        public async Task UpdateAsync_ValidClub_ReturnsUpdatedClub(int clubIndex)
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            var logger = new NullLogger<ClubService>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _validator = new ClubValidator(dbContext);
                _clubService = new ClubService(dbContext, mapper, logger, _validator);

                // Act
                CancellationToken token = new();
                var clubId = _clubs[clubIndex].ClubID;
                var updatedClub = new Incoming.ClubDto
                {
                    Name = "Updated Club Name",
                    ClubCode = "UPDATED"
                };

                var result = await _clubService.UpdateAsync(clubId, updatedClub, token);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.ClubID, Is.EqualTo(clubId));
                Assert.That(result.Name, Is.EqualTo(updatedClub.Name));
                Assert.That(result.ClubCode, Is.EqualTo(updatedClub.ClubCode));

                // Verify the club was updated in the database
                var savedClub = await dbContext.Clubs.FindAsync(clubId);
                Assert.That(savedClub, Is.Not.Null);
                Assert.That(savedClub.Name, Is.EqualTo(updatedClub.Name));
                Assert.That(savedClub.ClubCode, Is.EqualTo(updatedClub.ClubCode));
            }
        }

        [TestCase]
        public async Task UpdateAsync_NonExistentClub_ReturnsNull()
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            var logger = new NullLogger<ClubService>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _validator = new ClubValidator(dbContext);
                _clubService = new ClubService(dbContext, mapper, logger, _validator);

                // Act
                CancellationToken token = new();
                var nonExistentId = Guid.NewGuid();
                var updatedClub = new Incoming.ClubDto
                {
                    Name = "Non Existent Club",
                    ClubCode = "NONEXIST"
                };

                var result = await _clubService.UpdateAsync(nonExistentId, updatedClub, token);

                // Assert
                Assert.That(result, Is.Null);
            }
        }

        [TestCase(0)]
        public async Task UpdateAsync_DuplicateClubCode_ThrowsValidationException(int clubIndex)
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            var logger = new NullLogger<ClubService>();

            using (var dbContext = new PathfinderContext(ContextOptions))
            {
                _validator = new ClubValidator(dbContext);
                _clubService = new ClubService(dbContext, mapper, logger, _validator);

                // Act
                CancellationToken token = new();
                var clubId = _clubs[clubIndex].ClubID;
                var updatedClub = new Incoming.ClubDto
                {
                    Name = "Updated Club Name",
                    ClubCode = _clubs[1].ClubCode // Using another club's code to trigger duplicate exception
                };

                // Assert
                Assert.ThrowsAsync<ValidationException>(() => 
                    _clubService.UpdateAsync(clubId, updatedClub, token));
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            using var dbContext = new PathfinderContext(ContextOptions);
            await DatabaseCleaner.CleanDatabase(dbContext);
        }
    }
}
