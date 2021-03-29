// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using ClickerHeroesTrackerWebsite.Models;
using Website.Services.Settings;

namespace Website.Models.Api.Users
{
    public sealed class UserSettings
    {
        /// <summary>
        /// Gets or sets the user's play style.
        /// </summary>
        public PlayStyle? PlayStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see numbers in scientific notation.
        /// </summary>
        public bool? UseScientificNotation { get; set; }

        /// <summary>
        /// Gets or sets the threshold at which to use scientific notation.
        /// </summary>
        public int? ScientificNotationThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see graphs with logarithmic scale.
        /// </summary>
        public bool? UseLogarithmicGraphScale { get; set; }

        /// <summary>
        /// Gets or sets the range a graph must cover to use logarithmic scale.
        /// </summary>
        public int? LogarithmicGraphScaleThreshold { get; set; }

        /// <summary>
        /// Gets or sets the hybrid idle:active ratio.
        /// </summary>
        public double? HybridRatio { get; set; }

        /// <summary>
        /// Gets or sets the theme type.
        /// </summary>
        public SiteThemeType? Theme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants suggestions for skill ancients.
        /// </summary>
        public bool? ShouldLevelSkillAncients { get; set; }

        /// <summary>
        /// Gets or sets the id of the ancient skill ancients should be based on.
        /// </summary>
        public int? SkillAncientBaseAncient { get; set; }

        /// <summary>
        /// Gets or sets the preferred number of levels the skill ancients should be in relation to the base ancient.
        /// </summary>
        public int? SkillAncientLevelDiff { get; set; }

        /// <summary>
        /// Gets or sets the graph spacing type to use.
        /// </summary>
        public GraphSpacingType? GraphSpacingType { get; set; }
    }
}
