using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PathfinderHonorManager.Models
{
    [Table("honor")]
    public class Honor
    {
        [Key, Column("honor_id")]
        public Guid HonorID { get; set; }
        [Column("name")]
        public String Name { get; set; }
        [Column("level")]
        public int Level { get; set; }
        [Column("description")]
        public String Description { get; set; }
        [Column("patch_path")]
        public Uri PatchPath { get; set; }
        [Column("wiki_path")]
        public Uri WikiPath { get; set; }
    }
}
