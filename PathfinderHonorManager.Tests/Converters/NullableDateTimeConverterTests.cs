using System;
using System.Text.Json;
using NUnit.Framework;
using PathfinderHonorManager.Converters;

namespace PathfinderHonorManager.Tests.Converters
{
    [TestFixture]
    public class NullableDateTimeConverterTests
    {
        private NullableDateTimeConverter _converter;

        [SetUp]
        public void Setup()
        {
            _converter = new NullableDateTimeConverter();
        }

        [Test]
        public void Read_NullString_ReturnsNull()
        {
            var json = "null";
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
            reader.Read();

            var result = _converter.Read(ref reader, typeof(DateTime?), new JsonSerializerOptions());

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Read_ValidDateTimeString_ReturnsDateTime()
        {
            var expectedDate = new DateTime(2024, 3, 14);
            var json = $"\"{expectedDate:O}\"";
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
            reader.Read();

            var result = _converter.Read(ref reader, typeof(DateTime?), new JsonSerializerOptions());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.Date, Is.EqualTo(expectedDate.Date));
        }

        [Test]
        public void Write_NullDateTime_WritesNullValue()
        {
            using var stream = new System.IO.MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            DateTime? nullDateTime = null;
            _converter.Write(writer, nullDateTime, new JsonSerializerOptions());
            writer.Flush();

            var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void Write_MinValueDateTime_WritesNullValue()
        {
            using var stream = new System.IO.MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            DateTime? minValue = DateTime.MinValue;
            _converter.Write(writer, minValue, new JsonSerializerOptions());
            writer.Flush();

            var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void Write_ValidDateTime_WritesDateTimeString()
        {
            using var stream = new System.IO.MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            var dateTime = new DateTime(2024, 3, 14);
            _converter.Write(writer, dateTime, new JsonSerializerOptions());
            writer.Flush();

            var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            Assert.That(json, Is.EqualTo($"\"{dateTime:yyyy-MM-ddTHH:mm:ss}\""));
        }
    }
} 