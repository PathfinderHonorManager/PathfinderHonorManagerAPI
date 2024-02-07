using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Model;

namespace PathfinderHonorManager.DataAccess
{
    public class PathfinderContext : DbContext
    {
        public PathfinderContext(DbContextOptions<PathfinderContext> options)
            : base(options)
        {
        }

        public DbSet<Pathfinder> Pathfinders { get; set; }
        public DbSet<PathfinderClass> PathfinderClasses { get; set; }
        public DbSet<Honor> Honors { get; set; }
        public DbSet<PathfinderHonor> PathfinderHonors { get; set; }
        public DbSet<PathfinderHonorStatus> PathfinderHonorStatuses { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PathfinderAchievement> PathfinderAchievements { get; set; }
    }
}