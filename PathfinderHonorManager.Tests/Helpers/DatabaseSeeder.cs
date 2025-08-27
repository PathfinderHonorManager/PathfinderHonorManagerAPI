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
            // Check if Clubs already exist to avoid duplicates
            if (dbContext.Clubs.Any())
            {
                _clubs = await dbContext.Clubs.ToListAsync();
                return;
            }

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
                    FirstName = "Bob",
                    LastName = "TheSixthGrader",
                    Email = "BSG@example.com",
                    Grade = 6,
                    ClubID = _clubs[0].ClubID,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    IsActive = true
                },
                new Pathfinder
                {
                    PathfinderID = Guid.NewGuid(),
                    FirstName = "Sally",
                    LastName = "Seven",
                    Email = "SallySeven@example.com",
                    Grade = 7,
                    ClubID = _clubs[0].ClubID,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    IsActive = true
                },
                new Pathfinder
                {
                    PathfinderID = Guid.NewGuid(),
                    FirstName = "Chuck",
                    LastName = "Eight",
                    Email = "chuckeight@example.com",
                    Grade = 8,
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
            // Check if PathfinderHonorStatuses already exist to avoid duplicates
            if (dbContext.PathfinderHonorStatuses.Any())
            {
                _pathfinderHonorStatuses = await dbContext.PathfinderHonorStatuses.ToListAsync();
                return;
            }

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
                    CategorySequenceOrder = 1
                },
                new Category
                {
                    CategoryID = Guid.NewGuid(),
                    CategoryName = "Test Category 2",
                    CategorySequenceOrder = 2
                }
            };

            await dbContext.Categories.AddRangeAsync(_categories);
            await dbContext.SaveChangesAsync();
        }

        public static async Task SeedAchievements(PathfinderContext dbContext)
        {
            _achievements = new List<Achievement>();

            for (int grade = 5; grade <= 10; grade++)
            {
                _achievements.Add(new Achievement
                {
                    AchievementID = Guid.NewGuid(),
                    Grade = grade,
                    Level = 1,
                    Description = $"Achievement Level 1 for Grade {grade}",
                    CategoryID = _categories[0].CategoryID 
                });

                _achievements.Add(new Achievement
                {
                    AchievementID = Guid.NewGuid(),
                    Grade = grade,
                    Level = 2,
                    Description = $"Achievement Level 2 for Grade {grade}",
                    CategoryID = _categories[1].CategoryID
                });
            }

            await dbContext.Achievements.AddRangeAsync(_achievements);
            await dbContext.SaveChangesAsync();
        }
        public static async Task SeedPathfinderAchievements(PathfinderContext dbContext)
        {
            if (_achievements == null || !_achievements.Any() || _pathfinders == null || !_pathfinders.Any())
            {
                throw new InvalidOperationException("Achievements or Pathfinders not seeded properly.");
            }

            var random = new Random();
            _pathfinderAchievements = new List<PathfinderAchievement>();

            foreach (var pathfinder in _pathfinders)
            {
                var level1Achievements = _achievements.Where(a => a.Level == 1).ToList();
                var level2Achievements = _achievements.Where(a => a.Level == 2).ToList();

                // Shuffle the achievements to randomize the order
                Shuffle(level1Achievements, random);
                Shuffle(level2Achievements, random);

                // Determine a random number of achievements to assign from each level
                int level1AchievementsToAssignCount = random.Next(1, level1Achievements.Count + 1);
                int level2AchievementsToAssignCount = random.Next(1, level2Achievements.Count + 1);

                // Take the determined number of achievements from each level and add them
                foreach (var achievement in level1Achievements.Take(level1AchievementsToAssignCount))
                {
                    _pathfinderAchievements.Add(new PathfinderAchievement
                    {
                        PathfinderAchievementID = Guid.NewGuid(),
                        AchievementID = achievement.AchievementID,
                        PathfinderID = pathfinder.PathfinderID,
                        IsAchieved = random.Next(2) == 1 
                    });
                }

                foreach (var achievement in level2Achievements.Take(level2AchievementsToAssignCount))
                {
                    _pathfinderAchievements.Add(new PathfinderAchievement
                    {
                        PathfinderAchievementID = Guid.NewGuid(),
                        AchievementID = achievement.AchievementID,
                        PathfinderID = pathfinder.PathfinderID,
                        IsAchieved = random.Next(2) == 1 
                    });
                }
            }

            // Fisher-Yates shuffle algorithm
            void Shuffle<T>(IList<T> list, Random rng)
            {
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }

            await dbContext.PathfinderAchievements.AddRangeAsync(_pathfinderAchievements);
            await dbContext.SaveChangesAsync();
        }
        public static async Task SeedPathfinderClasses(PathfinderContext dbContext)
        {
            var pathfinderClasses = new List<PathfinderClass>();

            for (int grade = 5; grade <= 12; grade++)
            {
                pathfinderClasses.Add(new PathfinderClass
                {
                    Grade = grade,
                    ClassName = $"Class for Grade {grade}",
                });
            }

            await dbContext.PathfinderClasses.AddRangeAsync(pathfinderClasses);
            await dbContext.SaveChangesAsync();
        }

    }
}