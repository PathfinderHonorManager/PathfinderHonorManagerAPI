using System;
using System.ComponentModel.DataAnnotations;

namespace PathfinderHonorManager.Dto.Incoming
{
    public class PathfinderAchievementDto
    {
        public Guid PathfinderID { get; set; }
        public Guid AchievementID { get; set; }
        public bool IsAchieved { get; set; }
    }
    public class PostPathfinderAchievementDto
    {
        [Required]
        public Guid AchievementID { get; set; }

    }

}
