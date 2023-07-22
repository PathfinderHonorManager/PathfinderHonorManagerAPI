using System;
using System.Collections.Generic;

namespace PathfinderHonorManager.Dto.Outgoing
{
    public class PathfinderDto
    {
        public Guid PathfinderID { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; }

        public int? Grade {get; set;}

        public string? ClassName { get; set;}

        public bool? IsActive { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }

    public class PathfinderDependantDto
    {
        public Guid PathfinderID { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; }

        public int? Grade {get; set;}

        public string? ClassName { get; set;}

        public bool? IsActive { get; set; }

        public string ClubName { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public ICollection<PathfinderHonorDto>? PathfinderHonors { get; set; }
    }
}
