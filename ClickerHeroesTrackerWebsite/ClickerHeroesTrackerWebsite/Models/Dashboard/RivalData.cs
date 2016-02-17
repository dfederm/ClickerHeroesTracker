// <copyright file="RivalData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    /// <summary>
    /// Data describing a rival.
    /// </summary>
    public class RivalData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RivalData"/> class.
        /// </summary>
        public RivalData(int rivalId, string userName)
        {
            this.RivalId = rivalId;
            this.UserName = userName;
        }

        /// <summary>
        /// Gets the rilalry id.
        /// </summary>
        public int RivalId { get; }

        /// <summary>
        /// Gets the rival's user name.
        /// </summary>
        public string UserName { get; }
}
}