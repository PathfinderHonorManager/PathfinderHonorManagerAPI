using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PathfinderHonorManager.Models
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

        [Column("create_timestamp"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Created { get; set; }
        [ForeignKey("HonorFk")]
        public Honor Honor { get; set; }
        [Column("pathfinder_id")]
        public Guid PathfinderFk { get; set; }

    }


    [Table("pathfinder_honor_status")]
        public class PathfinderHonorStatus
    {
        [Key, Column("status_code")]
        public int StatusCode { get; set; }
        [Column("name")]
        public String Status { get; set; }
    }
}
