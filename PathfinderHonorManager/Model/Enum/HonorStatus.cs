using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PathfinderHonorManager.Model.Enum
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HonorStatus
    {
        Planned = 100,
        Earned = 200,
        Awarded = 300
    }
}


