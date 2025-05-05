using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Tests.Helpers;
using PathfinderHonorManager.Validators;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Tests.Service
{
    public class HonorServiceTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        private HonorService _honorService;
        private PathfinderContext _dbContext;
        private List<Honor> _honors;
        private IMapper _mapper;
        private IValidator<Incoming.HonorDto> _validator;

        [SetUp]
        public async Task SetUp()
        {
            _dbContext = new PathfinderContext(SharedContextOptions);
            await DatabaseCleaner.CleanDatabase(_dbContext);
            
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            _mapper = mapperConfiguration.CreateMapper();
            
            var logger = NullLogger<HonorService>.Instance;
            _validator = new HonorValidator(_dbContext);
            
            _honorService = new HonorService(_dbContext, _mapper, _validator, logger);
            _honors = await _dbContext.Honors.ToListAsync();
        }

        [Test]
        public async Task GetAllAsync_WithHonors_ReturnsAllHonors()
        {
            // Arrange
            var token = new CancellationToken();

            // Act
            var result = await _honorService.GetAllAsync(token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(_honors.Count));
            foreach (var honor in result)
            {
                var originalHonor = _honors.First(h => h.HonorID == honor.HonorID);
                Assert.That(honor.Name, Is.EqualTo(originalHonor.Name));
                Assert.That(honor.Level, Is.EqualTo(originalHonor.Level));
                Assert.That(honor.PatchFilename, Is.EqualTo(originalHonor.PatchFilename));
                Assert.That(honor.WikiPath, Is.EqualTo(originalHonor.WikiPath));
            }
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ReturnsHonor()
        {
            // Arrange
            var token = new CancellationToken();
            var expectedHonor = _honors.First();

            // Act
            var result = await _honorService.GetByIdAsync(expectedHonor.HonorID, token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HonorID, Is.EqualTo(expectedHonor.HonorID));
            Assert.That(result.Name, Is.EqualTo(expectedHonor.Name));
            Assert.That(result.Level, Is.EqualTo(expectedHonor.Level));
            Assert.That(result.PatchFilename, Is.EqualTo(expectedHonor.PatchFilename));
            Assert.That(result.WikiPath, Is.EqualTo(expectedHonor.WikiPath));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var token = new CancellationToken();
            var invalidId = Guid.NewGuid();

            // Act
            var result = await _honorService.GetByIdAsync(invalidId, token);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddAsync_WithValidData_ReturnsAddedHonor()
        {
            // Arrange
            var token = new CancellationToken();
            var newHonor = new Incoming.HonorDto
            {
                Name = "New Test Honor",
                Level = 1,
                PatchFilename = "new_test.jpg",
                WikiPath = new Uri("https://example.com/new")
            };

            // Act
            var result = await _honorService.AddAsync(newHonor, token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(newHonor.Name));
            Assert.That(result.Level, Is.EqualTo(newHonor.Level));
            Assert.That(result.PatchFilename, Is.EqualTo(newHonor.PatchFilename));
            Assert.That(result.WikiPath, Is.EqualTo(newHonor.WikiPath));

            // Verify it was added to the database
            var addedHonor = await _dbContext.Honors.FirstOrDefaultAsync(h => h.HonorID == result.HonorID);
            Assert.That(addedHonor, Is.Not.Null);
            Assert.That(addedHonor.Name, Is.EqualTo(newHonor.Name));
        }

        [Test]
        public async Task AddAsync_WithValidationError_ThrowsValidationException()
        {
            // Arrange
            var token = new CancellationToken();
            var newHonor = new Incoming.HonorDto
            {
                Name = string.Empty, // Invalid - name is required
                Level = 1,
                PatchFilename = "test.jpg",
                WikiPath = new Uri("https://example.com")
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _honorService.AddAsync(newHonor, token));
            Assert.That(ex.Message, Does.Contain("'Name' must not be empty"));
        }

        [Test]
        public async Task UpdateAsync_WithValidData_ReturnsUpdatedHonor()
        {
            // Arrange
            var token = new CancellationToken();
            var existingHonor = await _dbContext.Honors.AsNoTracking().FirstAsync(token);
            var updatedHonor = new Incoming.HonorDto
            {
                Name = "Updated Honor Name",
                Level = 2,
                PatchFilename = "updated.jpg",
                WikiPath = new Uri("https://example.com/updated")
            };

            // Get a fresh copy of the entity to update
            var entityToUpdate = await _dbContext.Honors.FindAsync(existingHonor.HonorID);
            entityToUpdate.Name = updatedHonor.Name;
            entityToUpdate.Level = updatedHonor.Level;
            entityToUpdate.PatchFilename = updatedHonor.PatchFilename;
            entityToUpdate.WikiPath = updatedHonor.WikiPath;
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _honorService.UpdateAsync(existingHonor.HonorID, updatedHonor, token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HonorID, Is.EqualTo(existingHonor.HonorID));
            Assert.That(result.Name, Is.EqualTo(updatedHonor.Name));
            Assert.That(result.Level, Is.EqualTo(updatedHonor.Level));
            Assert.That(result.PatchFilename, Is.EqualTo(updatedHonor.PatchFilename));
            Assert.That(result.WikiPath, Is.EqualTo(updatedHonor.WikiPath));

            // Verify it was updated in the database by getting a fresh copy
            var dbHonor = await _dbContext.Honors.AsNoTracking().FirstOrDefaultAsync(h => h.HonorID == existingHonor.HonorID);
            Assert.That(dbHonor, Is.Not.Null);
            Assert.That(dbHonor.Name, Is.EqualTo(updatedHonor.Name));
            Assert.That(dbHonor.Level, Is.EqualTo(updatedHonor.Level));
            Assert.That(dbHonor.PatchFilename, Is.EqualTo(updatedHonor.PatchFilename));
            Assert.That(dbHonor.WikiPath, Is.EqualTo(updatedHonor.WikiPath));
        }

        [Test]
        public async Task UpdateAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var token = new CancellationToken();
            var invalidId = Guid.NewGuid();
            var updatedHonor = new Incoming.HonorDto
            {
                Name = "Updated Honor",
                Level = 2,
                PatchFilename = "updated.jpg",
                WikiPath = new Uri("https://example.com/updated")
            };

            // Act
            var result = await _honorService.UpdateAsync(invalidId, updatedHonor, token);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateAsync_WithValidationError_ThrowsValidationException()
        {
            // Arrange
            var token = new CancellationToken();
            var existingHonor = _honors.First();
            var updatedHonor = new Incoming.HonorDto
            {
                Name = string.Empty, // Invalid - name is required
                Level = 2,
                PatchFilename = "updated.jpg",
                WikiPath = new Uri("https://example.com/updated")
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _honorService.UpdateAsync(existingHonor.HonorID, updatedHonor, token));
            Assert.That(ex.Message, Does.Contain("'Name' must not be empty"));
        }

        [TearDown]
        public async Task TearDown()
        {
            await DatabaseCleaner.CleanDatabase(_dbContext);
            _dbContext.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _dbContext.Dispose();
        }
    }
} 