using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PathfinderHonorManager.Dto.Incoming
{
    public class PathfinderDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        public int? Grade { get; set; }

        public bool? IsActive { get; set; }

    }

    public class PathfinderDtoInternal : PathfinderDto
    {
        public Guid? ClubID { get; set; }
    }

    public class PutPathfinderDto
    {
        public int? Grade { get; set; }

        public bool? IsActive { get; set; }
    }

    public class BulkPutPathfinderDto
    {
        [Required]
        public IEnumerable<BulkPutPathfinderItemDto> Items { get; set; }
    }

    public class BulkPutPathfinderItemDto
    {
        [Required]
        public Guid PathfinderId { get; set; }

        public int? Grade { get; set; }

        public bool? IsActive { get; set; }
    }
}