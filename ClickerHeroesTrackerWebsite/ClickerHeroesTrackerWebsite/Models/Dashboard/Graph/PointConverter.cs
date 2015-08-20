namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    using System;
    using Newtonsoft.Json;

    public sealed class PointConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Point).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var point = (Point)value;
            writer.WriteStartArray();
            writer.WriteRaw(point.X.ToString("F0"));
            writer.WriteRaw(",");
            writer.WriteRaw(point.Y.ToString("F0"));
            writer.WriteEndArray();
        }
    }
}