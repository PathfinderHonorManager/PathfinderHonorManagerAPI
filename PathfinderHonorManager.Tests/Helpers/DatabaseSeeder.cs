using System;
using System.Collections.Generic;
using System.Linq;
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
        private static List<Category> _categories;
        private static List<Achievement> _achievements;
        private static List<PathfinderAchievement> _pathfinderAchievements;


        public static async Task SeedDatabase(DbContextOptions<PathfinderContext> options)
        {
            using (var dbContext = new PathfinderContext(options))
            {
                await SeedClubs(dbContext);
                await SeedPathfinderHonorStatuses(dbContext);
                await SeedPathfinders(dbContext);
                await SeedHonors(dbContext);
                await SeedCategories(dbContext);
                await SeedPathfinderClasses(dbContext);
                await SeedAchievements(dbContext);
                await SeedPathfinderAchievements(dbContext);
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

        public static async Task SeedCategories(PathfinderContext dbContext)
        {
            _categories = new List<Category>
            {
                new Category
                {
                    CategoryID = Guid.NewGuid(),
                    CategoryName = "Test Category 1",
                },
                new Category
                {
                    CategoryID = Guid.NewGuid(),
                    CategoryName = "Test Category 2",
                }
            };

            await dbContext.Categories.AddRangeAsync(_categories);
            await dbContext.SaveChangesAsync();
        }

        public static async Task SeedAchievements(PathfinderContext dbContext)
        {
            _achievements = new List<Achievement>
            {
                new Achievement
                {
                    AchievementID = Guid.NewGuid(),
                    Grade = 5,
                    Level = 1,
                    Description = "Achievement 1 Description",
                    CategoryID = _categories[0].CategoryID
                },
                new Achievement
                {
                    AchievementID = Guid.NewGuid(),
                    Grade = 5,
                    Level = 2,
                    Description = "Achievement 2 Description",
                    CategoryID = _categories[1].CategoryID
                }
            };

            await dbContext.Achievements.AddRangeAsync(_achievements);
            await dbContext.SaveChangesAsync();
        }
        public static async Task SeedPathfinderAchievements(PathfinderContext dbContext)
        {
            if (_achievements == null || !_achievements.Any() || _pathfinders == null || !_pathfinders.Any())
            {
                throw new InvalidOperationException("Achievements or Pathfinders not seeded properly.");
            }

            var _pathfinderAchievements = new List<PathfinderAchievement>
            {
                new PathfinderAchievement
                {
                    PathfinderAchievementID = Guid.NewGuid(),
                    AchievementID = _achievements[0].AchievementID,
                    PathfinderID = _pathfinders[0].PathfinderID,
                    IsAchieved = false
                },
                new PathfinderAchievement
                {
                    PathfinderAchievementID = Guid.NewGuid(),
                    AchievementID = _achievements[1].AchievementID,
                    PathfinderID = _pathfinders[1].PathfinderID,
                    IsAchieved = true
                },
            };

            await dbContext.PathfinderAchievements.AddRangeAsync(_pathfinderAchievements);
            await dbContext.SaveChangesAsync();
        }
        public static async Task SeedPathfinderClasses(PathfinderContext dbContext)
        {
            var pathfinderClasses = new List<PathfinderClass>
            {
                new PathfinderClass
                {
                    Grade = 5,
                    ClassName = "Class 1",
                },
                new PathfinderClass
                {
                    Grade = 6,
                    ClassName = "Class 2",
                }
            };

            await dbContext.PathfinderClasses.AddRangeAsync(pathfinderClasses);
            await dbContext.SaveChangesAsync();

        }

    }
}