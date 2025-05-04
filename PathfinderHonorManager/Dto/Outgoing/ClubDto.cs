using System;
using System.Diagnostics.CodeAnalysis;

namespace PathfinderHonorManager.Dto.Outgoing
{
    [ExcludeFromCodeCoverage]
    public class ClubDto
    {
        public Guid ClubID { get; set; }

        public string ClubCode { get; set; }

        public string Name { get; set; }
    }
}
