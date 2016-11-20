// <copyright file="GameData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the root game data object.
    /// </summary>
    /// <remarks>
    /// See GameData.json, last saved from ClickerHeroes_v6565.swf
    /// Instructions:
    /// Download the.swf from the link in the source code.
    /// Decompile using JPEXS Free Flash Decompiler
    /// Go to binary data folder and click on the only thing in there
    /// Click the open embedded swf message at the top of the hex editor
    /// Now expand that and go to the binary data folder within that, look for one that says staticdata.txt in the filename.
    /// Right-click, extract that file.
    /// open the .bin in a text editor.
    /// </remarks>
    [JsonObject]
    public class GameData
    {
        private static readonly JsonSerializer Serializer = CreateSerializer();

        /// <summary>
        /// Gets or sets a collection of ancients.
        /// </summary>
        [JsonProperty(PropertyName = "ancients", Required = Required.Always)]
        public IDictionary<int, Ancient> Ancients { get; set; }

        /// <summary>
        /// Gets or sets a collection of item bonus types.
        /// </summary>
        [JsonProperty(PropertyName = "itemBonusTypes", Required = Required.Always)]
        public IDictionary<int, ItemBonusType> ItemBonusTypes { get; set; }

        /// <summary>
        /// Parses a <see cref="GameData"/> from a json file.
        /// </summary>
        /// <param name="file">The file with the data</param>
        /// <returns>An <see cref="GameData"/> object</returns>
        public static GameData Parse(string file)
        {
            using (var reader = new StreamReader(file))
            {
                return Serializer.Deserialize<GameData>(new JsonTextReader(reader));
            }
        }

        private static JsonSerializer CreateSerializer()
        {
            var settings = new JsonSerializerSettings();
            return JsonSerializer.Create(settings);
        }
    }
}
