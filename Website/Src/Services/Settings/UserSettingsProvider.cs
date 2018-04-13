// <copyright file="UserSettingsProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using Website.Models.Api.Users;

    /// <summary>
    /// An <see cref="IUserSettingsProvider"/> implementation which uses a database as the backing store.
    /// </summary>
    public class UserSettingsProvider : IUserSettingsProvider
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettingsProvider"/> class.
        /// </summary>
        public UserSettingsProvider(IDatabaseCommandFactory databaseCommandFactory)
        {
            this.databaseCommandFactory = databaseCommandFactory;
        }

        /// <inheritdoc/>
        public async Task<UserSettings> GetAsync(string userId)
        {
            // If the user isn't logged in, use the default settings
            if (string.IsNullOrEmpty(userId))
            {
                return new UserSettings();
            }

            var userSettings = new UserSettings();

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
            };
            const string GetUserSettingsCommandText = @"
                SELECT SettingId, SettingValue
                FROM UserSettings
                WHERE UserId = @UserId";
            using (var command = this.databaseCommandFactory.Create(
                GetUserSettingsCommandText,
                parameters))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var settingId = Convert.ToByte(reader["SettingId"]);
                    var settingValue = reader["SettingValue"].ToString();

                    switch (settingId)
                    {
                        case UserSettingsConstants.AreUploadsPublic:
                            userSettings.AreUploadsPublic = bool.TryParse(settingValue, out var areUploadsPublic) ? new bool?(areUploadsPublic) : null;
                            break;
                        case UserSettingsConstants.PlayStyle:
                            userSettings.PlayStyle = Enum.TryParse<PlayStyle>(settingValue, out var playStyle) ? new PlayStyle?(playStyle) : null;
                            break;
                        case UserSettingsConstants.UseScientificNotation:
                            userSettings.UseScientificNotation = bool.TryParse(settingValue, out var useScientificNotation) ? new bool?(useScientificNotation) : null;
                            break;
                        case UserSettingsConstants.ScientificNotationThreshold:
                            userSettings.ScientificNotationThreshold = int.TryParse(settingValue, out var scientificNotationThreshold) ? new int?(scientificNotationThreshold) : null;
                            break;
                        case UserSettingsConstants.UseLogarithmicGraphScale:
                            userSettings.UseLogarithmicGraphScale = bool.TryParse(settingValue, out var useLogarithmicGraphScale) ? new bool?(useLogarithmicGraphScale) : null;
                            break;
                        case UserSettingsConstants.LogarithmicGraphScaleThreshold:
                            userSettings.LogarithmicGraphScaleThreshold = int.TryParse(settingValue, out var logarithmicGraphScaleThreshold) ? new int?(logarithmicGraphScaleThreshold) : null;
                            break;
                        case UserSettingsConstants.HybridRatio:
                            userSettings.HybridRatio = double.TryParse(settingValue, out var hybridRatio) ? new double?(hybridRatio) : null;
                            break;
                        case UserSettingsConstants.Theme:
                            userSettings.Theme = Enum.TryParse<SiteThemeType>(settingValue, out var theme) ? new SiteThemeType?(theme) : null;
                            break;
                        case UserSettingsConstants.ShouldLevelSkillAncients:
                            userSettings.ShouldLevelSkillAncients = bool.TryParse(settingValue, out var shouldLevelSkillAncients) ? new bool?(shouldLevelSkillAncients) : null;
                            break;
                        case UserSettingsConstants.SkillAncientBaseAncient:
                            userSettings.SkillAncientBaseAncient = int.TryParse(settingValue, out var skillAncientBaseAncient) ? new int?(skillAncientBaseAncient) : null;
                            break;
                        case UserSettingsConstants.SkillAncientLevelDiff:
                            userSettings.SkillAncientLevelDiff = int.TryParse(settingValue, out var skillAncientLevelDiff) ? new int?(skillAncientLevelDiff) : null;
                            break;
                    }
                }
            }

            return userSettings;
        }

        /// <inheritdoc/>
        public async Task PatchAsync(string userId, UserSettings userSettings)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (userSettings == null)
            {
                throw new ArgumentNullException(nameof(userSettings));
            }

            /* Build a query that looks like this:
                MERGE INTO UserSettings WITH (HOLDLOCK)
                USING
                    (VALUES (@UserId, 1, @Value1), (@UserId, 2, @Value2), ... )
                        AS Input(UserId, SettingId, SettingValue)
                    ON UserSettings.UserId = Input.UserId
                    AND UserSettings.SettingId = Input.SettingId
                WHEN MATCHED THEN
                    UPDATE
                    SET
                        SettingValue = Input.SettingValue
                WHEN NOT MATCHED THEN
                    INSERT (UserId, SettingId, SettingValue)
                    VALUES (Input.UserId, Input.SettingId, Input.SettingValue);");
            */
            var setUserSettingsCommandText = new StringBuilder();
            var parameters = new Dictionary<string, object>()
            {
                { "@UserId", userId },
            };

            var isFirst = true;

            setUserSettingsCommandText.Append(@"
                MERGE INTO UserSettings WITH (HOLDLOCK)
                USING
                    ( VALUES ");

            void AppendSetting(byte settingId, string settingValue)
            {
                if (string.IsNullOrEmpty(settingValue))
                {
                    return;
                }

                if (!isFirst)
                {
                    setUserSettingsCommandText.Append(",");
                }

                // No need to sanitize settingId as it's just a number
                setUserSettingsCommandText.Append("(@UserId,");
                setUserSettingsCommandText.Append(settingId);
                setUserSettingsCommandText.Append(",@Value");
                setUserSettingsCommandText.Append(settingId);
                setUserSettingsCommandText.Append(")");

                parameters.Add("@Value" + settingId, settingValue);

                isFirst = false;
            }

            AppendSetting(UserSettingsConstants.AreUploadsPublic, userSettings.AreUploadsPublic?.ToString());
            AppendSetting(UserSettingsConstants.PlayStyle, userSettings.PlayStyle?.ToString());
            AppendSetting(UserSettingsConstants.UseScientificNotation, userSettings.UseScientificNotation?.ToString());
            AppendSetting(UserSettingsConstants.ScientificNotationThreshold, userSettings.ScientificNotationThreshold?.ToString());
            AppendSetting(UserSettingsConstants.UseLogarithmicGraphScale, userSettings.UseLogarithmicGraphScale?.ToString());
            AppendSetting(UserSettingsConstants.LogarithmicGraphScaleThreshold, userSettings.LogarithmicGraphScaleThreshold?.ToString());
            AppendSetting(UserSettingsConstants.HybridRatio, userSettings.HybridRatio?.ToString());
            AppendSetting(UserSettingsConstants.Theme, userSettings.Theme?.ToString());
            AppendSetting(UserSettingsConstants.ShouldLevelSkillAncients, userSettings.ShouldLevelSkillAncients?.ToString());
            AppendSetting(UserSettingsConstants.SkillAncientBaseAncient, userSettings.SkillAncientBaseAncient?.ToString());
            AppendSetting(UserSettingsConstants.SkillAncientLevelDiff, userSettings.SkillAncientLevelDiff?.ToString());

            // If no settings were appended, just short-circuit.
            if (isFirst)
            {
                return;
            }

            setUserSettingsCommandText.Append(@"
                    )
                        AS Input(UserId, SettingId, SettingValue)
                    ON UserSettings.UserId = Input.UserId
                    AND UserSettings.SettingId = Input.SettingId
                WHEN MATCHED THEN
                    UPDATE
                    SET
                        SettingValue = Input.SettingValue
                WHEN NOT MATCHED THEN
                    INSERT (UserId, SettingId, SettingValue)
                    VALUES (Input.UserId, Input.SettingId, Input.SettingValue);");
            using (var command = this.databaseCommandFactory.Create(
                setUserSettingsCommandText.ToString(),
                parameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}