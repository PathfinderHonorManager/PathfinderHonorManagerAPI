using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PathfinderHonorManager.Dto.Incoming
{
    [ExcludeFromCodeCoverage]
    public class ClubDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string ClubCode { get; set; }
    }
} 