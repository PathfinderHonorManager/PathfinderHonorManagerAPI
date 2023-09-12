using System;
using Castle.Components.DictionaryAdapter;
using System.Text.Json.Serialization;
using PathfinderHonorManager.Converters;

namespace PathfinderHonorManager.Dto.Outgoing
{
    public class PathfinderHonorDto
    {
        public Guid PathfinderHonorID { get; set; }

        public Guid PathfinderID { get; set; }

        public Guid HonorID { get; set; }
        public string Name { get; set; }

        public string Status { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? Earned { get; set; }

        public string PatchFilename { get; set;}

        public string WikiPath { get; set;}
    }
}