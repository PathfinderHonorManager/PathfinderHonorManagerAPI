using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PathfinderHonorManager.Model;

namespace PathfinderHonorManager.Tests.Integration
{
    public class TestModelCustomizer : RelationalModelCustomizer
    {
        public TestModelCustomizer(ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            modelBuilder.Entity<Pathfinder>()
                .Property(p => p.Updated)
                .ValueGeneratedNever();

            modelBuilder.Entity<PathfinderHonor>()
                .Property(p => p.Created)
                .ValueGeneratedNever();

            modelBuilder.Entity<PathfinderAchievement>()
                .Property(p => p.CreateTimestamp)
                .ValueGeneratedNever();
        }
    }
}
