namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System.Collections.Generic;

    public class RivalDataList
    {
        public RivalDataList(string userId)
        {
            var rivals = new List<RivalData>();

            using (var command = new DatabaseCommand("GetUserRivals"))
            {
                command.AddParameter("@UserId", userId);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var rivalId = (int)reader["Id"];
                    var rivalUserName = (string)reader["RivalUserName"];
                    var rivalData = new RivalData(rivalId, rivalUserName);
                    rivals.Add(rivalData);
                }
            }

            this.Rivals = rivals;
            this.IsValid = rivals.Count > 0;
        }

        public bool IsValid { get; private set; }

        public IList<RivalData> Rivals { get; private set; }
    }
}