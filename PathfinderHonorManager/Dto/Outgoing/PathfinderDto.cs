using System;
using System.Collections.Generic;

namespace PathfinderHonorManager.Dto.Outgoing
{
    public class PathfinderDto
    {
        public Guid PathfinderID { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        //public String Email { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

    }

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

}
