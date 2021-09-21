using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;


namespace PathfinderHonorManager
{
    [Table("pathfinder_honor")]
    public class PathfinderHonor
    {
        [Key, Column("pathfinder_honor_id")]
        public Guid PathfinderHonorID { get; set; }

        [Column("honor_id")]
        public Guid HonorFk { get; set; }
        [Column("status_code")]
        public int StatusCode { get; set; }
        [Column("create_timestamp")]
        public DateTime Created { get; set; }
        [ForeignKey("HonorFk")]
        public virtual Honor Honor { get; set; }
        [Column("pathfinder_id")]
        public Guid PathfinderFk { get; set; }

    }
}
