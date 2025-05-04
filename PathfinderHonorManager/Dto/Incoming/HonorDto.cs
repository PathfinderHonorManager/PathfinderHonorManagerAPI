﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace PathfinderHonorManager.Dto.Incoming
{
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

