using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PathfinderHonorManager.Model
{
    [Table("club")]
    public class Club
    {
        [Key, Column("club_id")]
        public Guid ClubID { get; set; }

        [Column("club_code")]
        public String ClubCode { get; set; }

        [Column("name")]
        public String Name { get; set; }
    }
}
