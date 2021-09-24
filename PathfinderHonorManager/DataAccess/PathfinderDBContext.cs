using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Models;

namespace PathfinderHonorManager.DataAccess
{
    public class PathfinderContext : DbContext
    {
        public DbSet<Pathfinder> Pathfinders { get; set; }

        public DbSet<Honor> Honors { get; set; }

        public DbSet<PathfinderHonor> PathfinderHonors { get; set; }

        public DbSet<PathfinderHonorStatus> PathfinderHonorStatuses { get; set; }

        public PathfinderContext(DbContextOptions<PathfinderContext> options)
        : base(options)
        {
        }
    }

}