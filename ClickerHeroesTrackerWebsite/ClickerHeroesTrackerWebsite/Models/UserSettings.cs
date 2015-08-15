namespace ClickerHeroesTrackerWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public class UserSettings
    {
        private delegate bool TryParse<T>(string rawValue, out T value);

        private readonly Dictionary<byte, string> settingValues = new Dictionary<byte, string>();

        private readonly HashSet<byte> dirtySettings = new HashSet<byte>();

        private readonly string userId;

        public UserSettings(string userId)
        {
            this.userId = userId;
        }

        public void Fill()
        {
            using (var command = new DatabaseCommand("GetUserSettings"))
            {
                command.AddParameter("@UserId", this.userId);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var settingId = (byte)reader["SettingId"];
                    var settingValue = (string)reader["SettingValue"];
                    settingValues.Add(settingId, settingValue);
                }
            }
        }

        public void Save()
        {
            if (this.dirtySettings.Count == 0)
            {
                return;
            }

            using (var command = new DatabaseCommand("SetUserSettings"))
            {
                command.AddParameter("@UserId", this.userId);

                DataTable settingsTable = new DataTable();
                settingsTable.Columns.Add("SettingId", typeof(byte));
                settingsTable.Columns.Add("SettingValue", typeof(string));
                foreach (var settingId in this.dirtySettings)
                {
                    settingsTable.Rows.Add(settingId, this.settingValues[settingId]);
                }

                command.AddTableParameter("@UserSettings", "UserSetting", settingsTable);

                command.ExecuteNonQuery();
            }

            this.dirtySettings.Clear();
        }

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
                if (!value.Equals(oldValue))
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