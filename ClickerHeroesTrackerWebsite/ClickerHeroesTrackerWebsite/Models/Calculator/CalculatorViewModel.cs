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
        private static readonly JsonSerializer serializer = CreateSerializer();

        public CalculatorViewModel(string encodedSaveData, IIdentity user, bool addToProgress)
        {
            // Decode the save
            var jsonData = DecodeSaveData(encodedSaveData);
            if (jsonData == null)
            {
                return;
            }

            // Deserialize the save
            var savedGame = DeserializeSavedGame(jsonData);
            if (savedGame == null)
            {
                return;
            }

            var userId = user.GetUserId();
            this.UserSettings = new UserSettings(userId);
            this.UserSettings.Fill();

            // Finally, populate the view models
            this.IsPublic = this.UserSettings.AreUploadsPublic;
            this.IsOwn = this.IsPermitted = this.IsValid = true;

            this.UploadUserName = user.GetUserName();
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

                    // Ancient levels
                    DataTable ancientLevelTable = new DataTable();
                    ancientLevelTable.Columns.Add("AncientId", typeof(int));
                    ancientLevelTable.Columns.Add("Level", typeof(int));
                    foreach (var pair in this.AncientLevelSummaryViewModel.AncientLevels)
                    {
                        ancientLevelTable.Rows.Add(pair.Key.Id, pair.Value);
                    }

                    command.AddTableParameter("@AncientLevelUploads", "AncientLevelUpload", ancientLevelTable);

                    var returnParameter = command.AddReturnParameter();

                    command.ExecuteNonQuery();

                    this.UploadId = (int)returnParameter.Value;
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
                    var uploadUserId = (string)reader["UserId"];
                    var uploadUserName = (string)reader["UserName"];
                    var uploadTime = (DateTime)reader["UploadTime"];
                    ////var uploadContent = (string)reader["UploadContent"];

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

        private static JsonSerializer CreateSerializer()
        {
            var settings = new JsonSerializerSettings();
            settings.Error += (sender, args) =>
            {
                // Just swallow
                args.ErrorContext.Handled = true;
            };

            return JsonSerializer.Create(settings);
        }

        internal static byte[] DecodeSaveData(string encodedSaveData)
        {
            const string AntiCheatCode = "Fe12NAfA3R6z4k0z";
            var antiCheatCodeIndex = encodedSaveData.IndexOf(AntiCheatCode);
            if (antiCheatCodeIndex == -1)
            {
                // Couldn't find anti-cheat
                return null;
            }

            // Remove every other character, AKA "unsprinkle"
            var unsprinkledChars = new char[antiCheatCodeIndex / 2];
            for (var i = 0; i < antiCheatCodeIndex; i += 2)
            {
                unsprinkledChars[i / 2] = encodedSaveData[i];
            }

            // Validation
            const string Salt = "af0ik392jrmt0nsfdghy0";
            var expectedHashStart = antiCheatCodeIndex + AntiCheatCode.Length;
            var saltedChars = new char[unsprinkledChars.Length + Salt.Length];
            unsprinkledChars.CopyTo(saltedChars, 0);
            Salt.CopyTo(0, saltedChars, unsprinkledChars.Length, Salt.Length);
            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(saltedChars));
                var actualHash = new StringBuilder(data.Length * 2);
                for (int i = 0; i < data.Length; i++)
                {
                    var expectedHashPartIndex = expectedHashStart + (i * 2);
                    var actualHashPart = data[i].ToString("x2");
                    if (actualHashPart[0] != encodedSaveData[expectedHashPartIndex]
                        || actualHashPart[1] != encodedSaveData[expectedHashPartIndex + 1])
                    {
                        // Hash didn't match
                        return null;
                    }
                }
            }

            // Decode and return
            return Convert.FromBase64CharArray(unsprinkledChars, 0, unsprinkledChars.Length);
        }

        internal static SavedGame DeserializeSavedGame(byte[] saveData)
        {
            using (var stream = new MemoryStream(saveData))
            {
                using (var reader = new StreamReader(stream))
                {
                    return serializer.Deserialize<SavedGame>(new JsonTextReader(reader));
                }
            }
        }
    }
}