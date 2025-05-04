using System;
using System.Diagnostics.CodeAnalysis;

namespace PathfinderHonorManager.Dto.Outgoing
{
    [ExcludeFromCodeCoverage]
    public class HonorDto
    {
        public Guid HonorID { get; set; }

        public string Name { get; set; }

        public int Level { get; set; }

        public string PatchFilename { get; set; }

        public Uri WikiPath { get; set; }
    }
}
