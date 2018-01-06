// <copyright file="UserSettings.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Users
{
    using ClickerHeroesTrackerWebsite.Models;

    public sealed class UserSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user's uploads are public.
        /// </summary>
        public bool? AreUploadsPublic { get; set; }

        /// <summary>
        /// Gets or sets the user's play style.
        /// </summary>
        public PlayStyle? PlayStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see numbers in scientific notation
        /// </summary>
        public bool? UseScientificNotation { get; set; }

        /// <summary>
        /// Gets or sets the threshold at which to use scientific notation
        /// </summary>
        public int? ScientificNotationThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the effective level is used for suggestions vs the actual ancient levels.
        /// </summary>
        public bool? UseEffectiveLevelForSuggestions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see graphs with logarithmic scale
        /// </summary>
        public bool? UseLogarithmicGraphScale { get; set; }

        /// <summary>
        /// Gets or sets the range a graph must cover to use logarithmic scale
        /// </summary>
        public int? LogarithmicGraphScaleThreshold { get; set; }

        /// <summary>
        /// Gets or sets the hybrid idle:active ratio
        /// </summary>
        public double? HybridRatio { get; set; }

        /// <summary>
        /// Gets or sets the theme type.
        /// </summary>
        public SiteThemeType? Theme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants suggestions for skill ancients
        /// </summary>
        public bool? ShouldLevelSkillAncients { get; set; }

        /// <summary>
        /// Gets or sets the id of the ancient skill ancients should be based on
        /// </summary>
        public int? SkillAncientBaseAncient { get; set; }

        /// <summary>
        /// Gets or sets the preferred number of levels the skill ancients should be in relation to the base ancient.
        /// </summary>
        public int? SkillAncientLevelDiff { get; set; }
    }
}
