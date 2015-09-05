namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using SaveData;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;
    using System.Data;
    using System.Security.Principal;
    using Microsoft.AspNet.Identity;

    public class CalculatorViewModel
    {
        public CalculatorViewModel(string encodedSaveData, IIdentity user, bool addToProgress)
        {
            var savedGame = SavedGame.Parse(encodedSaveData);
            if (savedGame == null)
            {
                return;
            }

            var userId = user?.GetUserId();
            this.UserSettings = new UserSettings(userId);
            this.UserSettings.Fill();

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
                using (var command = new DatabaseCommand("UploadSaveData"))
                {
                    // Upload data
                    command.AddParameter("@UserId", userId);
                    command.AddParameter("@UploadContent", encodedSaveData);

                    // Computed stats
                    command.AddParameter("@OptimalLevel", this.ComputedStatsViewModel.OptimalLevel);
                    command.AddParameter("@SoulsPerHour", this.ComputedStatsViewModel.SoulsPerHour);
                    command.AddParameter("@SoulsPerAscension", this.ComputedStatsViewModel.OptimalSoulsPerAscension);
                    command.AddParameter("@AscensionTime", this.ComputedStatsViewModel.OptimalAscensionTime);
                    command.AddParameter("@TitanDamage", this.ComputedStatsViewModel.TitanDamage);
                    command.AddParameter("@SoulsSpent", this.ComputedStatsViewModel.SoulsSpent);

                    // Ancient levels
                    DataTable ancientLevelTable = new DataTable();
                    ancientLevelTable.Columns.Add("AncientId", typeof(int));
                    ancientLevelTable.Columns.Add("Level", typeof(long));
                    foreach (var pair in this.AncientLevelSummaryViewModel.AncientLevels)
                    {
                        ancientLevelTable.Rows.Add(pair.Key.Id, pair.Value);
                    }

                    command.AddTableParameter("@AncientLevelUploads", "AncientLevelUpload", ancientLevelTable);

                    var returnParameter = command.AddReturnParameter();

                    command.ExecuteNonQuery();

                    this.UploadId = Convert.ToInt32(returnParameter.Value);
                }
            }
        }

        public CalculatorViewModel(int uploadId, string userId)
        {
            this.UserSettings = new UserSettings(userId);
            this.UserSettings.Fill();

            this.UploadId = uploadId;

            using (var command = new DatabaseCommand("GetUploadDetails"))
            {
                command.AddParameter("@UploadId", uploadId);

                var reader = command.ExecuteReader();

                // General upload data
                if (reader.Read())
                {
                    var uploadUserId = reader["UserId"].ToString();
                    var uploadUserName = reader["UserName"].ToString();
                    var uploadTime = Convert.ToDateTime(reader["UploadTime"]);
                    ////var uploadContent = reader["UploadContent"].ToString();

                    this.IsOwn = userId == uploadUserId;
                    if (this.IsOwn)
                    {
                        this.IsPublic = this.UserSettings.AreUploadsPublic;
                        this.IsPermitted = true;
                    }
                    else
                    {
                        var uploadUserSettings = new UserSettings(uploadUserId);
                        uploadUserSettings.Fill();

                        this.IsPublic = this.IsPermitted = uploadUserSettings.AreUploadsPublic;
                    }

                    this.UploadUserName = uploadUserName;
                    this.UploadTime = uploadTime;
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

                this.SuggestedAncientLevelsViewModel = new SuggestedAncientLevelsViewModel(
                    this.AncientLevelSummaryViewModel.AncientLevels,
                    this.ComputedStatsViewModel.OptimalLevel,
                    this.UserSettings);

                this.IsValid = true;
            }
        }

        public UserSettings UserSettings { get; private set; }

        public bool IsOwn { get; private set; }

        public bool IsPermitted { get; private set; }

        public bool IsPublic { get; private set; }

        public bool IsValid { get; private set; }

        public int UploadId { get; private set; }

        public string UploadUserName { get; private set; }

        public DateTime UploadTime { get; private set; }

        public AncientLevelSummaryViewModel AncientLevelSummaryViewModel { get; private set; }

        ////public HeroLevelSummaryViewModel HeroLevelSummaryViewModel { get; private set; }

        public ComputedStatsViewModel ComputedStatsViewModel { get; private set; }

        public SuggestedAncientLevelsViewModel SuggestedAncientLevelsViewModel { get; private set; }
    }
}