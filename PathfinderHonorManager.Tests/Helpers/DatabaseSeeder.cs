using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Model.Enum;

namespace PathfinderHonorManager.Tests.Helpers
{
    public static class DatabaseSeeder
    {
        private static List<Honor> _honors;
        private static List<Pathfinder> _pathfinders;
        private static List<Club> _clubs;

        private static List<PathfinderHonorStatus> _pathfinderHonorStatuses;

        public static async Task SeedDatabase(DbContextOptions<PathfinderContext> options)
        {
            using (var dbContext = new PathfinderContext(options))
            {
                await SeedClubs(dbContext);
                await SeedPathfinderHonorStatuses(dbContext);
                await SeedPathfinders(dbContext);
                await SeedHonors(dbContext);
                await SeedPathfinderHonors(dbContext);
            }
        }

        public static async Task SeedClubs(PathfinderContext dbContext)
        {
            _clubs = new List<Club>
            {
                new Club
                {
                    ClubID = Guid.NewGuid(),
                    ClubCode = "VALIDCLUBCODE"
                },
                new Club
                {
                    ClubID = Guid.NewGuid(),
                    ClubCode = "EMPTYCLUB"
                }
            };

            await dbContext.Clubs.AddRangeAsync(_clubs);
            await dbContext.SaveChangesAsync();
        }
        public static async Task SeedPathfinders(PathfinderContext dbContext)
        {
            _pathfinders = new List<Pathfinder>
            {
                new Pathfinder
                {
                    PathfinderID = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "johndoe@example.com",
                    Grade = 5,
                    ClubID = _clubs[0].ClubID,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    IsActive = true
                },
                new Pathfinder
                {
                    PathfinderID = Guid.NewGuid(),
                    FirstName = "Addy",
                    LastName = "Addsome",
                    Email = "addyaddsome@example.com",
                    Grade = 9,
                    ClubID = _clubs[0].ClubID,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    IsActive = true
                },
                new Pathfinder
                {
                    PathfinderID = Guid.NewGuid(),
                    FirstName = "Inactive",
                    LastName = "Pathfinder",
                    Email = "inactive@example.com",
                    Grade = 10,
                    ClubID = _clubs[0].ClubID,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    IsActive = false
                }
            };

            await dbContext.Pathfinders.AddRangeAsync(_pathfinders);
            await dbContext.SaveChangesAsync();
        }

        public static async Task SeedHonors(PathfinderContext dbContext)
        {
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

            await dbContext.Honors.AddRangeAsync(_honors);
            await dbContext.SaveChangesAsync();
        }
        public static async Task SeedPathfinderHonorStatuses(PathfinderContext dbContext)
        {
            _pathfinderHonorStatuses = new List<PathfinderHonorStatus>
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

            await dbContext.PathfinderHonorStatuses.AddRangeAsync(_pathfinderHonorStatuses);
            await dbContext.SaveChangesAsync();
        }

        public static async Task SeedPathfinderHonors(PathfinderContext dbContext)
        {
            var pathfinderHonors = new List<PathfinderHonor>
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

            await dbContext.PathfinderHonors.AddRangeAsync(pathfinderHonors);
            await dbContext.SaveChangesAsync();
        }
    }
}
