// <copyright file="IUserSettings.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    /// <summary>
    /// The type of site theme
    /// </summary>
    public enum SiteThemeType
    {
        /// <summary>
        /// Default light theme.
        /// </summary>
        Light,

        /// <summary>
        /// Dark theme for contrast.
        /// </summary>
        Dark,
    }

    /// <summary>
    /// The user's persistent site settings
    /// </summary>
    public interface IUserSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user's uploads are public.
        /// </summary>
        bool AreUploadsPublic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reduced solomon formula (Log) as opposed to the more optimal one (Ln).
        /// </summary>
        bool UseReducedSolomonFormula { get; set; }

        /// <summary>
        /// Gets or sets the user's play style.
        /// </summary>
        PlayStyle PlayStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see experimental stats
        /// </summary>
        bool UseExperimentalStats { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see numbers in scientific notation
        /// </summary>
        bool UseScientificNotation { get; set; }

        /// <summary>
        /// Gets or sets the threshold at which to use scientific notation
        /// </summary>
        int ScientificNotationThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the effective level is used for suggestions vs the actual ancient levels.
        /// </summary>
        bool UseEffectiveLevelForSuggestions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see graphs with logarithmic scale
        /// </summary>
        bool UseLogarithmicGraphScale { get; set; }

        /// <summary>
        /// Gets or sets the range a graph must cover to use logarithmic scale
        /// </summary>
        int LogarithmicGraphScaleThreshold { get; set; }

        /// <summary>
        /// Gets or sets the hybrid idle:active ratio
        /// </summary>
        int HybridRatio { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the theme type.
        /// </summary>
        SiteThemeType Theme { get; set; }
    }
}