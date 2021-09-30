using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PathfinderHonorManager.Model.Enum
{
    public class HonorStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum AssetType
        {
            Planned = 100,
            Earned = 200,
            Awarded = 300
        }
    }
}


