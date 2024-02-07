using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PathfinderHonorManager.Model
{
    [Table("category")]
    public class Category
    {
        [Key, Column("category_id")]
        public Guid CategoryID { get; set; }

        [Column("name")]
        public string CategoryName { get; set; }
    }
}
