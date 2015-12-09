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

    /// <summary>
    /// The top level data and configuration for a graph.
    /// </summary>
    /// <remarks>
    /// Structure defined here: http://api.highcharts.com/highcharts
    /// </remarks>
    [JsonObject]
    public class GraphData
    {
        private static readonly JsonSerializer Serializer = CreateSerializer();

        /// <summary>
        /// Gets or sets the options regarding the chart area and plot area as well as general chart options.
        /// </summary>
        public Chart Chart { get; set; }

        /// <summary>
        /// Gets or sets the chart's main title.
        /// </summary>
        public Title Title { get; set; }

        /// <summary>
        /// Gets or sets the X axis or category axis configuration.
        /// </summary>
        public Axis XAxis { get; set; }

        /// <summary>
        /// Gets or sets the X axis or value axis configuration.
        /// </summary>
        public Axis YAxis { get; set; }

        /// <summary>
        /// Gets or sets the legend configuration, a box containing a symbol and name for each series item or point item in the chart.
        /// </summary>
        public Legend Legend { get; set; }

        /// <summary>
        /// Gets or sets a list of actual series to append to the chart.
        /// </summary>
        public IList<Series> Series { get; set; }

        /// <summary>
        /// Serializes this obect to a json object string.
        /// </summary>
        /// <returns>A json string which represents this object's data</returns>
        public string ToJsonString()
        {
            using (var writer = new StringWriter())
            {
                Serializer.Serialize(writer, this);
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