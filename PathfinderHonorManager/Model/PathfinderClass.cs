using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PathfinderHonorManager.Model
{
    [Table("pathfinder_class")]
    public class PathfinderClass
    {
        [Key, Column("grade")]
        public int Grade { get; set; }
        [Column("name")]
        public String ClassName { get; set; }
    }
}
