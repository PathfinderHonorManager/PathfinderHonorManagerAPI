using System;

namespace PathfinderHonorManager.Dto.Outgoing
{
    public class PathfinderHonorDto
    {
        public Guid PathfinderID { get; set; }
        public Guid HonorID { get; set; }
        public String Name { get; set; }
        public string Status { get; set; }
    }
}

