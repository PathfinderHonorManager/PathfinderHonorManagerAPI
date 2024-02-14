using System;
using Newtonsoft.Json;
using PathfinderHonorManager.Converters;

namespace PathfinderHonorManager.Dto.Outgoing
{
    public class PathfinderAchievementDto
    {
        public AchievementDto Achievement { get; set; }
        public Guid PathfinderAchievementID { get; set; }
        public Guid PathfinderID { get; set; }
        public Guid AchievementID { get; set; }
        public bool IsAchieved { get; set; }
        public DateTime CreateTimestamp { get; set; }
        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? AchieveTimestamp { get; set; }
    }
}
