using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PathfinderHonorManager.Dto.Incoming
{
    public class PathfinderDto
    {
        [Required]
        public String FirstName { get; set; }
        [Required]
        public String LastName { get; set; }
        [Required]
        public String Email { get; set; }

    }
/*
    public class PathfinderDependantDto
    {
        public Guid PathfinderID { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        //public String Email { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public ICollection<PathfinderHonorDto> PathfinderHonors { get; set; }
    }
*/
}
