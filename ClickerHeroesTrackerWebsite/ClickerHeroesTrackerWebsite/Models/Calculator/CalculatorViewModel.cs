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

    public class CalculatorViewModel
    {
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

            this.AncientLevelSummaryViewModel = new AncientLevelSummaryViewModel(savedGame.AncientsData);
            ////this.HeroLevelSummaryViewModel = new HeroLevelSummaryViewModel(savedGame.HeroesData);
            this.ComputedStatsViewModel = new ComputedStatsViewModel(savedGame, this.UserSettings);
            this.SuggestedAncientLevelsViewModel = new SuggestedAncientLevelsViewModel(
                this.AncientLevelSummaryViewModel.AncientLevels,
                this.ComputedStatsViewModel.OptimalLevel,
                this.UserSettings);

            if (addToProgress && user.IsAuthenticated)
            {
                using (var command = databaseCommandFactory.Create(
                    "UploadSaveData",
                    CommandType.StoredProcedure,
                    new Dictionary<string, object>
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
                    }))
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

            using (var command = databaseCommandFactory.Create(
                "GetUploadDetails",
                CommandType.StoredProcedure,
                new Dictionary<string, object>
                {
                    { "@UploadId", uploadId }
                }))
            using (var reader = command.ExecuteReader())
            {
                // General upload data
                if (reader.Read())
                {
                    uploadUserId = reader["UserId"].ToString();
                    this.UploadUserName = reader["UserName"].ToString(); ;
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

                this.IsOwn = userId == uploadUserId;
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
        }

        public IUserSettings UserSettings { get; private set; }

        public bool IsOwn { get; private set; }

        public bool IsPermitted { get; private set; }

        public bool IsPublic { get; private set; }

        public bool IsValid { get; private set; }

        public int UploadId { get; private set; }

        public string UploadUserName { get; private set; }

        public DateTime UploadTime { get; private set; }

        public string UploadContent { get; private set; }

        public AncientLevelSummaryViewModel AncientLevelSummaryViewModel { get; private set; }

        ////public HeroLevelSummaryViewModel HeroLevelSummaryViewModel { get; private set; }

        public ComputedStatsViewModel ComputedStatsViewModel { get; private set; }

        public SuggestedAncientLevelsViewModel SuggestedAncientLevelsViewModel { get; private set; }
    }
}