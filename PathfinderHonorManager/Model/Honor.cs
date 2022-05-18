using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PathfinderHonorManager.Model
{
    [Table("honor")]
    public class Honor
    {
        [Key, Column("honor_id")]
        public Guid HonorID { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("level")]
        public int Level { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("patch_path")]
        public string PatchFilename { get; set; }
        [Column("wiki_path")]
        public Uri WikiPath { get; set; }
    }
}
