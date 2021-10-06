using System;
using System.ComponentModel.DataAnnotations;

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
}
