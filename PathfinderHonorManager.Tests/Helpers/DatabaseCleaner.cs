using PathfinderHonorManager.DataAccess;
using System.Linq;
using System.Threading.Tasks;

namespace PathfinderHonorManager.Tests.Helpers
{
    public static class DatabaseCleaner
    {
        public static async Task CleanDatabase(PathfinderContext dbContext)
        {
            if (dbContext.Pathfinders.Any())
                dbContext.Pathfinders.RemoveRange(dbContext.Pathfinders);
            if (dbContext.Honors.Any())
                dbContext.Honors.RemoveRange(dbContext.Honors);
            if (dbContext.PathfinderHonors.Any())
                dbContext.PathfinderHonors.RemoveRange(dbContext.PathfinderHonors);
            if (dbContext.PathfinderHonorStatuses.Any())
                dbContext.PathfinderHonorStatuses.RemoveRange(dbContext.PathfinderHonorStatuses);
            if (dbContext.Clubs.Any())
                dbContext.Clubs.RemoveRange(dbContext.Clubs);
            if (dbContext.Achievements.Any())
                dbContext.Achievements.RemoveRange(dbContext.Achievements);
            if (dbContext.Categories.Any())
                dbContext.Categories.RemoveRange(dbContext.Categories);
            if (dbContext.PathfinderClasses.Any())
                dbContext.PathfinderClasses.RemoveRange(dbContext.PathfinderClasses);
            if (dbContext.PathfinderAchievements.Any())
                dbContext.PathfinderAchievements.RemoveRange(dbContext.PathfinderAchievements);

            await dbContext.SaveChangesAsync();
        }
    }
}
