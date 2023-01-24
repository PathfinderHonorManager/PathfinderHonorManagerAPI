using System;
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

    }

    public class PathfinderDtoInternal : PathfinderDto
    {
        public Guid? ClubID { get; set; }
    }

    public class PutPathfinderDto
    {
        public int? Grade { get; set; }
    }
}
