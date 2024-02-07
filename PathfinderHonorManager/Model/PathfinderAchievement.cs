using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PathfinderHonorManager.Model
{
    [Table("pathfinder_achievement")]
    public class PathfinderAchievement
    {
        [Key, Column("pathfinder_achievement_id")]
        public Guid PathfinderAchievementID { get; set; }

        [Column("pathfinder_id")]
        public Guid PathfinderID { get; set; }

        [Column("achievement_id")]
        public Guid AchievementID { get; set; }

        [Column("is_achieved")]
        public bool IsAchieved { get; set; } = false;

        [Column("create_timestamp"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTimestamp { get; set; }

        [Column("achieve_timestamp")]
        public DateTime? AchieveTimestamp { get; set; }

        [ForeignKey("AchievementID")]
        public Achievement Achievement { get; set; }

        [ForeignKey("PathfinderID")]
        public Pathfinder Pathfinder { get; set; }
    }
}
