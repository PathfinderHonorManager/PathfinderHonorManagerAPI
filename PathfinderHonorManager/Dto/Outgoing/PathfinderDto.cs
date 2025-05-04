#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PathfinderHonorManager.Dto.Outgoing
{
    [ExcludeFromCodeCoverage]
    public class PathfinderDto
    {
        public Guid PathfinderID { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int? Grade { get; set; }

        public string? ClassName { get; set; }

        public bool? IsActive { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class PathfinderDependantDto
    {
        public Guid PathfinderID { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int? Grade { get; set; }

        public string? ClassName { get; set; }

        public bool? IsActive { get; set; }

        public string ClubName { get; set; } = null!;

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public int AssignedBasicAchievementsCount { get; set; }

        public int CompletedBasicAchievementsCount { get; set; }
        public int AssignedAdvancedAchievementsCount { get; set; }

        public int CompletedAdvancedAchievementsCount { get; set; }

        public ICollection<PathfinderHonorDto>? PathfinderHonors { get; set; }

    }
}
