// <copyright file="CalculatorViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Database;
    using Game;
    using Microsoft.ApplicationInsights;
    using SaveData;
    using Settings;
    using System.Security.Claims;

    /// <summary>
    /// The model for the calculator view.
    /// </summary>
    public class CalculatorViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorViewModel"/> class.
        /// </summary>
        public CalculatorViewModel(
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            GameData gameData,
            TelemetryClient telemetryClient,
            int uploadId,
            ClaimsPrincipal user)
        {
            var userId = user.GetUserId();
            this.UserSettings = userSettingsProvider.Get(userId);

            this.UploadId = uploadId;

            string uploadUserId;

            var parameters = new Dictionary<string, object>
            {
                { "@UploadId", uploadId },
            };
            using (var command = databaseCommandFactory.Create(
                "GetUploadDetails",
                CommandType.StoredProcedure,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                // General upload data
                if (reader.Read())
                {
                    uploadUserId = reader["UserId"].ToString();
                    this.UploadUserName = reader["UserName"].ToString();
                    this.UploadTime = Convert.ToDateTime(reader["UploadTime"]);
                    this.UploadContent = reader["UploadContent"].ToString();
                }
                else
                {
                    return;
                }

                if (!reader.NextResult())
                {
                    return;
                }

                // Get ancient levels
                var savedGame = SavedGame.Parse(this.UploadContent);
                this.AncientLevelSummaryViewModel = new AncientLevelSummaryViewModel(
                    gameData,
                    savedGame,
                    telemetryClient);

                // Skip passed the ancient levels. They're not used anymore.
                // BUGBUG 43 - Move off stored procedures
                if (!reader.NextResult())
                {
                    return;
                }

                this.ComputedStatsViewModel = new ComputedStatsViewModel(reader, this.UserSettings);
            }

            var isUploadAnonymous = string.IsNullOrEmpty(uploadUserId);
            this.IsOwn = string.Equals(userId, uploadUserId, StringComparison.OrdinalIgnoreCase);
            if (this.IsOwn)
            {
                this.IsPublic = isUploadAnonymous || this.UserSettings.AreUploadsPublic;
                this.IsPermitted = true;
            }
            else
            {
                var uploadUserSettings = userSettingsProvider.Get(uploadUserId);

                this.IsPublic = isUploadAnonymous || uploadUserSettings.AreUploadsPublic;
                this.IsPermitted = this.IsPublic || user.IsInRole("Admin");
            }

            this.SuggestedAncientLevelsViewModel = new SuggestedAncientLevelsViewModel(
                gameData,
                this.AncientLevelSummaryViewModel.AncientLevels,
                this.ComputedStatsViewModel.OptimalLevel,
                this.UserSettings);

            if (this.UserSettings.UseExperimentalStats)
            {
                this.ExperimentalStatsViewModel = new ExperimentalStatsViewModel(
                    gameData,
                    this.AncientLevelSummaryViewModel.AncientLevels,
                    this.UserSettings);
            }

            this.IsValid = true;
        }

        /// <summary>
        /// Gets the current user settings.
        /// </summary>
        public IUserSettings UserSettings { get; }

        /// <summary>
        /// Gets a value indicating whether the upload is the user's own upload.
        /// </summary>
        public bool IsOwn { get; }

        /// <summary>
        /// Gets a value indicating whether the user is permitted to view the upload.
        /// </summary>
        public bool IsPermitted { get; }

        /// <summary>
        /// Gets a value indicating whether the upload is public.
        /// </summary>
        public bool IsPublic { get; }

        /// <summary>
        /// Gets a value indicating whether the model is valid
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the upload id.
        /// </summary>
        public int UploadId { get; }

        /// <summary>
        /// Gets the name of the user who owns the upload.
        /// </summary>
        public string UploadUserName { get; }

        /// <summary>
        /// Gets the time the upload was submitted.
        /// </summary>
        public DateTime UploadTime { get; }

        /// <summary>
        /// Gets the encoded upload data.
        /// </summary>
        public string UploadContent { get; }

        /// <summary>
        /// Gets a model for the ancient level summary view.
        /// </summary>
        public AncientLevelSummaryViewModel AncientLevelSummaryViewModel { get; }

        ////public HeroLevelSummaryViewModel HeroLevelSummaryViewModel { get; }

        /// <summary>
        /// Gets a model for the computer stats view.
        /// </summary>
        public ComputedStatsViewModel ComputedStatsViewModel { get; }

        /// <summary>
        /// Gets a model for the suggested ancient levels view.
        /// </summary>
        public SuggestedAncientLevelsViewModel SuggestedAncientLevelsViewModel { get; }

        /// <summary>
        /// Gets a model for the experimental stats view.
        /// </summary>
        public ExperimentalStatsViewModel ExperimentalStatsViewModel { get; }
    }
}