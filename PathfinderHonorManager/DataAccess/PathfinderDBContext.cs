using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Model;
using System.Diagnostics.CodeAnalysis;

namespace PathfinderHonorManager.DataAccess
{
    [ExcludeFromCodeCoverage]
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure delete behavior to prevent unwanted cascade deletes
            
            // Pathfinder -> Club: Restrict (don't delete pathfinders when club is deleted)
            modelBuilder.Entity<Pathfinder>()
                .HasOne(p => p.Club)
                .WithMany()
                .HasForeignKey(p => p.ClubID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pathfinder>()
                .Property(p => p.FirstName)
                .IsRequired();

            modelBuilder.Entity<Pathfinder>()
                .Property(p => p.LastName)
                .IsRequired();

            modelBuilder.Entity<Pathfinder>()
                .Property(p => p.Email)
                .IsRequired(false);

            // Pathfinder -> PathfinderClass: Restrict (nullable relationship already)
            modelBuilder.Entity<Pathfinder>()
                .HasOne(p => p.PathfinderClass)
                .WithMany()
                .HasForeignKey(p => p.Grade)
                .OnDelete(DeleteBehavior.Restrict);

            // PathfinderAchievement relationships: Consider what should happen
            // Option 1: Cascade (if pathfinder deleted, remove their achievements)
            // Option 2: Restrict (prevent deletion if achievements exist)
            modelBuilder.Entity<PathfinderAchievement>()
                .HasOne(pa => pa.Pathfinder)
                .WithMany(p => p.PathfinderAchievements)
                .HasForeignKey(pa => pa.PathfinderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PathfinderAchievement>()
                .HasOne(pa => pa.Achievement)
                .WithMany()
                .HasForeignKey(pa => pa.AchievementID)
                .OnDelete(DeleteBehavior.Restrict);

            // PathfinderHonor relationships
            modelBuilder.Entity<PathfinderHonor>()
                .HasOne<Pathfinder>()
                .WithMany(p => p.PathfinderHonors)
                .HasForeignKey(ph => ph.PathfinderID)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<PathfinderHonor>()
                .HasOne(ph => ph.Honor)
                .WithMany()
                .HasForeignKey(ph => ph.HonorID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent honor deletion if in use

            modelBuilder.Entity<PathfinderHonor>()
                .HasOne(ph => ph.PathfinderHonorStatus)
                .WithMany()
                .HasForeignKey(ph => ph.StatusCode)
                .OnDelete(DeleteBehavior.Restrict); // Prevent status deletion if in use

            // Achievement -> Category: Restrict
            modelBuilder.Entity<Achievement>()
                .HasOne(a => a.Category)
                .WithMany()
                .HasForeignKey(a => a.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Achievement -> PathfinderClass: Restrict
            modelBuilder.Entity<Achievement>()
                .HasOne(a => a.PathfinderClass)
                .WithMany()
                .HasForeignKey(a => a.Grade)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}