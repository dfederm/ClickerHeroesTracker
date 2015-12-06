namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using Database;
    using System;
    using System.Collections.Generic;

    public class RivalDataList
    {
        public RivalDataList(string userId)
        {
            var rivals = new List<RivalData>();

            // BUGBUG 57 - Use IDatabaseCommandFactory
            using (var command = new SqlDatabaseCommand("GetUserRivals"))
            {
                command.AddParameter("@UserId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rivalId = Convert.ToInt32(reader["Id"]);
                        var rivalUserName = reader["RivalUserName"].ToString();
                        var rivalData = new RivalData(rivalId, rivalUserName);
                        rivals.Add(rivalData);
                    }
                }
            }

            this.Rivals = rivals;
            this.IsValid = rivals.Count > 0;
        }

        public bool IsValid { get; private set; }

        public IList<RivalData> Rivals { get; private set; }
    }
}