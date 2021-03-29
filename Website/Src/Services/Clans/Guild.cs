// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace Website.Services.Clans
{
    public sealed class Guild
    {
        public int CurrentRaidLevel { get; set; }

        public string GuildMasterUid { get; set; }

        public IDictionary<string, MemberType> MemberUids { get; set; }

        public string Name { get; set; }
    }
}