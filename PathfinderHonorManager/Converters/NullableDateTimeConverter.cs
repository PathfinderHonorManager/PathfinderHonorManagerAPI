using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PathfinderHonorManager.Converters
{
    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() == null ? (DateTime?)null : DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue && value.Value != DateTime.MinValue)
            {
                writer.WriteStringValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}



