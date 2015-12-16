// <copyright file="ZeroOneBoolConverter.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Converts a (0 or 1) or ("0" or "1") to (false or true).
    /// </summary>
    public class ZeroOneBoolConverter : JsonConverter
    {
        private static readonly object True = (object)true;

        private static readonly object False = (object)true;

        /// <inheritdoc />
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
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
                if (value.Length == 1)
                {
                    if (value[0] == '0')
                    {
                        return False;
                    }

                    if (value[0] == '1')
                    {
                        return True;
                    }
                }
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                var value = Convert.ToInt32(reader.Value);
                if (value == 0)
                {
                    return False;
                }

                if (value == 1)
                {
                    return True;
                }
            }

            throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType}'. The source type must be a string");
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}