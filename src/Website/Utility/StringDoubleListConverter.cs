// <copyright file="StringDoubleListConverter.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Converts a comma-separated string to an array of ints.
    /// </summary>
    public class StringDoubleListConverter : JsonConverter
    {
        private static readonly char[] ItemDelimeter = new[] { ',' };

        /// <inheritdoc />
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double[]);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.String)
            {
                string value = reader.Value.ToString();
                var itemStrings = value.Split(ItemDelimeter);
                var items = new double[itemStrings.Length];
                for (var i = 0; i < itemStrings.Length; i++)
                {
                    items[i] = Convert.ToDouble(itemStrings[i]);
                }

                return items;
            }
            else
            {
                throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType}'. The source type must be a string");
            }
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}