using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

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

    public class PutPathfinderAchievementDto
    {
        public bool IsAchieved { get; set; }
    }

    public class PostPathfinderAchievementForGradeDto
    {
        [Required]
        public ICollection<Guid> PathfinderIds { get; set; }
    }

}
