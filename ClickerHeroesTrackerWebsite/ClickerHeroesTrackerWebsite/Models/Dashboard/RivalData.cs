// <copyright file="RivalData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    public class RivalData
    {
        public RivalData(int rivalId, string userName)
        {
            this.RivalId = rivalId;
            this.UserName = userName;
        }

        public int RivalId { get; private set; }

        public string UserName { get; private set; }
}
}