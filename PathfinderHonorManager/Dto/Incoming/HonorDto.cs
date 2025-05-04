using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PathfinderHonorManager.Dto.Incoming
{
    [ExcludeFromCodeCoverage]
    public class HonorDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        public string PatchFilename { get; set; }

        [Required]
        public Uri WikiPath { get; set; }
    }
}

