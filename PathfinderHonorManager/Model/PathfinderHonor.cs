using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PathfinderHonorManager.Model
{
    [Table("pathfinder_honor")]
    public class PathfinderHonor
    {
        [Key, Column("pathfinder_honor_id")]
        public Guid PathfinderHonorID { get; set; }

        [Column("honor_id")]
        public Guid HonorID { get; set; }
        [Column("status_code")]
        public int StatusCode { get; set; }
        [Column("create_timestamp"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Created { get; set; }
        [Column("pathfinder_id")]
        public Guid PathfinderID { get; set; }

        [ForeignKey("HonorID")]
        public Honor Honor { get; set; }

        [ForeignKey("StatusCode")]
        public PathfinderHonorStatus PathfinderHonorStatus { get; set; }


    }
}
