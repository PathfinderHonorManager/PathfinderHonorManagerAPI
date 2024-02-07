using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PathfinderHonorManager.Model.Enum;

namespace PathfinderHonorManager.Model
{
    [Table("achievement")]
    public class Achievement
    {
        [Key, Column("achievement_id")]
        public Guid AchievementID { get; set; }

        [Column("grade")]
        public int Grade { get; set; }

        [Column("level")]
        public int Level { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("category_id")]
        public Guid? CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public Category Category { get; set; }

        [ForeignKey("Grade")]
        public PathfinderClass PathfinderClass { get; set; }
    }
}
