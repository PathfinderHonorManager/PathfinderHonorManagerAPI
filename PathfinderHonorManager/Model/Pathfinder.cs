using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace PathfinderHonorManager
{
    [Table("pathfinder")]
    public class Pathfinder
    {
        [Key]
        [Column("pathfinder_id")]
        public Guid PathfinderID { get; set; }
        [Column("first_name")]
        public String FirstName { get; set; }
        [Column("last_name")]
        public String LastName { get; set; }
        [Column("email")]
        public String Email { get; set; }
        [Column("create_timestamp")]
        public DateTime Created { get; set; }
        [Column("update_timestamp")]
        public DateTime Updated { get; set; }

        [ForeignKey("PathfinderFk")]
        public virtual ICollection<PathfinderHonor> PathfinderHonors { get; set; }

    }
}
