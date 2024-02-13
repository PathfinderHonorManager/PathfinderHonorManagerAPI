using System;

namespace PathfinderHonorManager.Dto.Outgoing
{
    public class AchievementDto
    {
        public Guid AchievementID { get; set; }
        public int Level { get; set; }
        public string LevelName { get; set; }
        public int AchievementSequenceOrder { get; set; }
        public string ClassName { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public int CategorySequenceOrder { get; set; }
    }
}
