// <copyright file="GraphData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard.Graph
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    // Structure defined here: http://api.highcharts.com/highcharts
    [JsonObject]
    public class GraphData
    {
        private static readonly JsonSerializer serializer = CreateSerializer();

        public Chart Chart { get; set; }

        public Title Title { get; set; }

        public Axis XAxis { get; set; }

        public Axis YAxis { get; set; }

        public Legend Legend { get; set; }

        public IList<Series> Series { get; set; }

        public string ToJsonString()
        {
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }

        private static JsonSerializer CreateSerializer()
        {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            return JsonSerializer.CreateDefault(settings);
        }
    }
}