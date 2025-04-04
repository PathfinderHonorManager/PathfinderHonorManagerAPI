using System;
using System.ComponentModel.DataAnnotations;

namespace PathfinderHonorManager.Dto.Incoming
{
    public class ClubDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string ClubCode { get; set; }
    }
} 