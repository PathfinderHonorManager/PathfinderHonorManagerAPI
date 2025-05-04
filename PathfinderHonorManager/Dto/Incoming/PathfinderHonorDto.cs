using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace PathfinderHonorManager.Dto.Incoming
{
    public class PathfinderHonorDto
    {
        [Required]
        public Guid HonorID { get; set; }

        public Guid PathfinderID { get; set; }

        public int StatusCode { get; set; }

        [Required]
        public string Status { get; set; }
    }

    public class PostPathfinderHonorDto
    {
        [Required]
        public Guid HonorID { get; set; }

        [Required]
        public string Status { get; set; }
    }

    public class BulkPostPathfinderHonorDto
    {
        [Required]
        public Guid PathfinderID { get; set; }

        [Required]
        public IEnumerable<PostPathfinderHonorDto> Honors { get; set; }
    }

    public class PutPathfinderHonorDto
    {
        [Required]
        public string Status { get; set; }

        public Guid HonorID { get; set; }
    }

    public class BulkPutPathfinderHonorDto
    {
        [Required]
        public Guid PathfinderID { get; set; }

        [Required]
        public IEnumerable<PutPathfinderHonorDto> Honors { get; set; }
    }

}
