using System;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace PathfinderHonorManager.Dto.Outgoing
{
    public class HonorDto
    {
        public Guid HonorID { get; set; }

        public string Name { get; set; }

        public int Level { get; set; }

        public string PatchFilename { get; set; }

        public Uri WikiPath { get; set; }
    }
}
