using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PathfinderHonorManager.Model
{
    [Table("pathfinder_honor_status")]
    public class PathfinderHonorStatus
    {
        [Key, Column("status_code")]
        public int StatusCode { get; set; }
        [Column("name")]
        public String Status { get; set; }
        //[ForeignKey("StatusCode")]
        //public PathfinderHonor PathfinderHonor { get; set; }
    }
}
