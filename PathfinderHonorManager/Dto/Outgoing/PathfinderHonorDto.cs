using System;

namespace PathfinderHonorManager.Dto.Outgoing
{
    public class PathfinderHonorDto
    {
        public Guid PathfinderHonorID { get; set; }
        public Guid HonorID { get; set; }
        public String Name { get; set; }
        public string Status { get; set; }
        public int Level { get; set; }
        //public String Description { get; set; }
        public Uri PatchPath { get; set; }
        public Uri WikiPath { get; set; }
    }

    public class PathfinderHonorChildDto
    {
        public Guid PathfinderID { get; set; }
        public Guid PathfinderHonorID { get; set; }
        public Guid HonorID { get; set; }
        public String Name { get; set; }
        public string Status { get; set; }
        public int Level { get; set; }
        //public String Description { get; set; }
        public Uri PatchPath { get; set; }
        public Uri WikiPath { get; set; }
    }
}

