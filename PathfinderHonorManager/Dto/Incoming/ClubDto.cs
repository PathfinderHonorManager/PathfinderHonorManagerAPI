using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

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