// <copyright file="UserSettings.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Database;
    using Utility;

    internal sealed class UserSettings : IUserSettings
    {
        private readonly Dictionary<byte, string> settingValues = new Dictionary<byte, string>();

        private readonly HashSet<byte> dirtySettings = new HashSet<byte>();

        private readonly IDatabaseCommandFactory databaseCommandFactory;

        private readonly string userId;

        public UserSettings(
            IDatabaseCommandFactory databaseCommandFactory,
            string userId)
        {
            this.databaseCommandFactory = databaseCommandFactory;
            this.userId = userId;

            this.Fill();
        }

        public UserSettings()
        {
        }

        private delegate bool TryParse<T>(string rawValue, out T value);

        public TimeZoneInfo TimeZone
        {
            get
            {
                return this.GetValue(1, TryParseTimeZone, TimeZoneInfo.Utc);
            }

            set
            {
                this.SetValue(1, value.Id);
            }
        }

        public bool AreUploadsPublic
        {
            get
            {
                return this.GetValue(2, bool.TryParse, false);
            }

            set
            {
                this.SetValue(2, value.ToString());
            }
        }

        public bool UseReducedSolomonFormula
        {
            get
            {
                return this.GetValue(3, bool.TryParse, false);
            }

            set
            {
                this.SetValue(3, value.ToString());
            }
        }

        public PlayStyle PlayStyle
        {
            get
            {
                return this.GetValue(4, Enum.TryParse, PlayStyle.Idle);
            }

            set
            {
                this.SetValue(4, value.ToString());
            }
        }

        public bool UseExperimentalStats
        {
            get
            {
                return this.GetValue(5, bool.TryParse, false);
            }

            set
            {
                this.SetValue(5, value.ToString());
            }
        }

        public bool Prefer30MinuteRuns
        {
            get
            {
                return this.GetValue(6, bool.TryParse, false);
            }

            set
            {
                this.SetValue(6, value.ToString());
            }
        }

        internal void FlushChanges()
        {
            if (this.userId == null)
            {
                return;
            }

            if (this.dirtySettings.Count == 0)
            {
                return;
            }

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", this.userId },
            };
            using (var command = this.databaseCommandFactory.Create(
                "SetUserSettings",
                CommandType.StoredProcedure,
                parameters))
            {
                DataTable settingsTable = new DataTable();
                settingsTable.Columns.Add("SettingId", typeof(byte));
                settingsTable.Columns.Add("SettingValue", typeof(string));
                foreach (var settingId in this.dirtySettings)
                {
                    settingsTable.Rows.Add(settingId, this.settingValues[settingId]);
                }

                // BUGBUG 63 - Remove casts to SqlDatabaseCommand
                ((SqlDatabaseCommand)command).AddTableParameter("@UserSettings", "UserSetting", settingsTable);

                command.ExecuteNonQuery();
            }

            this.dirtySettings.Clear();
        }

        private static bool TryParseTimeZone(string value, out TimeZoneInfo timeZone)
        {
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(value);
                return true;
            }
            catch (TimeZoneNotFoundException)
            {
                timeZone = null;
                return false;
            }
        }

        private void Fill()
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", this.userId },
            };
            using (var command = this.databaseCommandFactory.Create(
                "GetUserSettings",
                CommandType.StoredProcedure,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var settingId = Convert.ToByte(reader["SettingId"]);
                    var settingValue = reader["SettingValue"].ToString();
                    this.settingValues.Add(settingId, settingValue);
                }
            }
        }

        private T GetValue<T>(byte settingId, TryParse<T> parser, T defaultValue)
        {
            string rawValue;
            T value;
            return this.settingValues.TryGetValue(settingId, out rawValue)
                && parser(rawValue, out value)
                ? value
                : defaultValue;
        }

        private void SetValue(byte settingId, string value)
        {
            if (value == null)
            {
                return;
            }

            bool isDirty = false;
            string oldValue;
            if (this.settingValues.TryGetValue(settingId, out oldValue))
            {
                if (!value.Equals(oldValue, StringComparison.Ordinal))
                {
                    this.settingValues[settingId] = value;
                    isDirty = true;
                }
            }
            else
            {
                this.settingValues.Add(settingId, value);
                isDirty = true;
            }

            if (isDirty)
            {
                this.dirtySettings.Add(settingId);
            }
        }
    }
}