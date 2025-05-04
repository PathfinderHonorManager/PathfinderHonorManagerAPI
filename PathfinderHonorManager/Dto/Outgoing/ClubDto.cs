using System;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace PathfinderHonorManager.Dto.Outgoing
{
    public class ClubDto
    {
        public Guid ClubID { get; set; }

        public string ClubCode { get; set; }

        public string Name { get; set; }
    }
}
