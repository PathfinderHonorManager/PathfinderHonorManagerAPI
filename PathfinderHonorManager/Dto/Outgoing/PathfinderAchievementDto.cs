using System;
using Newtonsoft.Json;
using PathfinderHonorManager.Converters;
using System.Diagnostics.CodeAnalysis;

namespace PathfinderHonorManager.Dto.Outgoing
{
    [ExcludeFromCodeCoverage]
    public class PathfinderAchievementDto
    {
        public Guid PathfinderAchievementID { get; set; }
        public Guid PathfinderID { get; set; }
        public Guid AchievementID { get; set; }
        public bool IsAchieved { get; set; }
        public DateTime CreateTimestamp { get; set; }
        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? AchieveTimestamp { get; set; }
        public int Level { get; set; }
        public string LevelName { get; set; }
        public int AchievementSequenceOrder { get; set; }
        public int Grade { get; set; }
        public string ClassName { get; set; }
        public int CategorySequenceOrder { get; set; }
    }
}
