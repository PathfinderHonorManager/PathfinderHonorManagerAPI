using System;
using System.Collections.Generic;

namespace PathfinderHonorManager.Dto.Outgoing
{
    public class PathfinderDto
    {
        public Guid PathfinderID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        // public string Email { get; set; }
        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }

    public class PathfinderDependantDto
    {
        public Guid PathfinderID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        // public string Email { get; set; }
        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public ICollection<PathfinderHonorDto> PathfinderHonors { get; set; }
    }
}
