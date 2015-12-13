// <copyright file="CalculatorViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Security.Principal;
    using Database;
    using Microsoft.AspNet.Identity;
    using SaveData;
    using Settings;

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
            string encodedSaveData,
            IIdentity user,
            bool addToProgress)
        {
            var savedGame = SavedGame.Parse(encodedSaveData);
            if (savedGame == null)
            {
                return;
            }

            var userId = user?.GetUserId();
            this.UserSettings = userSettingsProvider.Get(userId);

            // Finally, populate the view models
            this.IsPublic = this.UserSettings.AreUploadsPublic;
            this.IsOwn = this.IsPermitted = this.IsValid = true;

            this.UploadUserName = user?.GetUserName();
            this.UploadTime = DateTime.UtcNow;
            this.UploadContent = encodedSaveData;

            this.AncientLevelSummaryViewModel = new AncientLevelSummaryViewModel(savedGame.AncientsData);
            ////this.HeroLevelSummaryViewModel = new HeroLevelSummaryViewModel(savedGame.HeroesData);
            this.ComputedStatsViewModel = new ComputedStatsViewModel(savedGame, this.UserSettings);
            this.SuggestedAncientLevelsViewModel = new SuggestedAncientLevelsViewModel(
                this.AncientLevelSummaryViewModel.AncientLevels,
                this.ComputedStatsViewModel.OptimalLevel,
                this.UserSettings);

            if (addToProgress && user.IsAuthenticated)
            {
                var parameters = new Dictionary<string, object>
                {
                    // Upload data
                    { "@UserId", userId },
                    { "@UploadContent", encodedSaveData },

                    // Computed stats
                    { "@OptimalLevel", this.ComputedStatsViewModel.OptimalLevel },
                    { "@SoulsPerHour", this.ComputedStatsViewModel.SoulsPerHour },
                    { "@SoulsPerAscension", this.ComputedStatsViewModel.OptimalSoulsPerAscension },
                    { "@AscensionTime", this.ComputedStatsViewModel.OptimalAscensionTime },
                    { "@TitanDamage", this.ComputedStatsViewModel.TitanDamage },
                    { "@SoulsSpent", this.ComputedStatsViewModel.SoulsSpent },
                };
                using (var command = databaseCommandFactory.Create(
                    "UploadSaveData",
                    CommandType.StoredProcedure,
                    parameters))
                {
                    // Ancient levels
                    DataTable ancientLevelTable = new DataTable();
                    ancientLevelTable.Columns.Add("AncientId", typeof(int));
                    ancientLevelTable.Columns.Add("Level", typeof(long));
                    foreach (var pair in this.AncientLevelSummaryViewModel.AncientLevels)
                    {
                        ancientLevelTable.Rows.Add(pair.Key.Id, pair.Value);
                    }

                    // BUGBUG 63 - Remove casts to SqlDatabaseCommand
                    ((SqlDatabaseCommand)command).AddTableParameter("@AncientLevelUploads", "AncientLevelUpload", ancientLevelTable);

                    // BUGBUG 63 - Remove casts to SqlDatabaseCommand
                    var returnParameter = ((SqlDatabaseCommand)command).AddReturnParameter();

                    command.ExecuteNonQuery();

                    this.UploadId = Convert.ToInt32(returnParameter.Value);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorViewModel"/> class.
        /// </summary>
        public CalculatorViewModel(
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            int uploadId,
            IPrincipal user)
        {
            var userId = user.Identity.GetUserId();
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
                this.AncientLevelSummaryViewModel = new AncientLevelSummaryViewModel(reader);

                if (!reader.NextResult())
                {
                    return;
                }

                this.ComputedStatsViewModel = new ComputedStatsViewModel(reader, this.UserSettings);
            }

            this.IsOwn = string.Equals(userId, uploadUserId, StringComparison.OrdinalIgnoreCase);
            if (this.IsOwn)
            {
                this.IsPublic = this.UserSettings.AreUploadsPublic;
                this.IsPermitted = true;
            }
            else
            {
                var uploadUserSettings = userSettingsProvider.Get(uploadUserId);

                this.IsPublic = uploadUserSettings.AreUploadsPublic;
                this.IsPermitted = this.IsPublic || user.IsInRole("Admin");
            }

            this.SuggestedAncientLevelsViewModel = new SuggestedAncientLevelsViewModel(
                this.AncientLevelSummaryViewModel.AncientLevels,
                this.ComputedStatsViewModel.OptimalLevel,
                this.UserSettings);

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
    }
}