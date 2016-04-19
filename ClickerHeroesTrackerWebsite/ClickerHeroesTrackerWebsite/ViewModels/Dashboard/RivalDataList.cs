// <copyright file="RivalDataList.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Database;

    /// <summary>
    /// A model for the rival data list.
    /// </summary>
    public class RivalDataList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RivalDataList"/> class.
        /// </summary>
        public RivalDataList(
            IDatabaseCommandFactory databaseCommandFactory,
            string userId)
        {
            var rivals = new List<RivalData>();

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };
            using (var command = databaseCommandFactory.Create(
                "GetUserRivals",
                CommandType.StoredProcedure,
                parameters))
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

            this.Rivals = rivals;
            this.IsValid = rivals.Count > 0;
        }

        /// <summary>
        /// Gets a value indicating whether the model is valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the list of rival data.
        /// </summary>
        public IList<RivalData> Rivals { get; }
    }
}