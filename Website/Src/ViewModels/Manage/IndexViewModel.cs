// <copyright file="IndexViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// Model for the manage view.
    /// </summary>
    public class IndexViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user has a password set.
        /// </summary>
        public bool HasPassword { get; set; }

        /// <summary>
        /// Gets or sets a list of login providers the user has set up.
        /// </summary>
        public IList<UserLoginInfo> Logins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's uploads are public.
        /// </summary>
        [Display(Name = "Public uploads")]
        public bool AreUploadsPublic { get; set; }

        /// <summary>
        /// Gets or sets the user's preferred solomon formula (Log or Ln).
        /// </summary>
        [Display(Name = "Pre-transcendant Solomon formula")]
        public string SolomonFormula { get; set; }

        /// <summary>
        /// Gets or sets the user's preferred play style.
        /// </summary>
        [Display(Name = "Play Style")]
        public string PlayStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see experimental stats
        /// </summary>
        [Display(Name = "Experimental stats")]
        public bool UseExperimentalStats { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see numbers in scientific notation
        /// </summary>
        [Display(Name = "Scientific notation")]
        public bool UseScientificNotation { get; set; }

        /// <summary>
        /// Gets or sets the threshold at which to use scientific notation
        /// </summary>
        [Display(Name = "Scientific notation threshold")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer number")]
        public int? ScientificNotationThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the effective level is used for suggestions vs the actual ancient levels.
        /// </summary>
        [Display(Name = "Effective levels")]
        public bool UseEffectiveLevelForSuggestions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see graphs with logarithmic scale
        /// </summary>
        [Display(Name = "Logarithmic scale")]
        public bool UseLogarithmicGraphScale { get; set; }

        /// <summary>
        /// Gets or sets the range a graph must cover to use logarithmic scale
        /// </summary>
        [Display(Name = "Logarithmic scale threshold")]
        public int? LogarithmicGraphScaleThreshold { get; set; }

        /// <summary>
        /// Gets or sets the hybrid idle:active ratio
        /// </summary>
        [Display(Name = "Hybrid Ratio")]
        public int? HybridRatio { get; set; }

        /// <summary>
        /// Gets or sets the site theme type..
        /// </summary>
        [Display(Name = "Site theme")]
        public SiteThemeType Theme { get; set; }
    }
}