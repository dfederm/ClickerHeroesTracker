// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using ClickerHeroesTrackerWebsite.Models.Api.Clans;
using Newtonsoft.Json;
using Website.Services.Clans;
using Xunit;

namespace UnitTests.Models
{
    public static class GuildResponseTests
    {
        [Fact]
        public static void ComputedStats()
        {
            string responseString = TestData.ReadAllText("GuildResponse.txt");
            GuildResponse clanResponse = JsonConvert.DeserializeObject<GuildResponse>(responseString);

            Assert.NotNull(clanResponse);
            Assert.NotNull(clanResponse.Result);

            Assert.NotNull(clanResponse.Result.Guild);
            Assert.Equal("Some Guild Name", clanResponse.Result.Guild.Name);
            Assert.Equal("111111111111111111111111111111", clanResponse.Result.Guild.GuildMasterUid);

            Assert.Equal(10, clanResponse.Result.Guild.MemberUids.Count);
            Assert.Equal(MemberType.Member, clanResponse.Result.Guild.MemberUids["000000000000000000000000000000"]);
            Assert.Equal(MemberType.Member, clanResponse.Result.Guild.MemberUids["111111111111111111111111111111"]);
            Assert.Equal(MemberType.Member, clanResponse.Result.Guild.MemberUids["222222222222222222222222222222"]);
            Assert.Equal(MemberType.Member, clanResponse.Result.Guild.MemberUids["333333333333333333333333333333"]);
            Assert.Equal(MemberType.Member, clanResponse.Result.Guild.MemberUids["444444444444444444444444444444"]);
            Assert.Equal(MemberType.Member, clanResponse.Result.Guild.MemberUids["555555555555555555555555555555"]);
            Assert.Equal(MemberType.Member, clanResponse.Result.Guild.MemberUids["666666666666666666666666666666"]);
            Assert.Equal(MemberType.Member, clanResponse.Result.Guild.MemberUids["777777777777777777777777777777"]);
            Assert.Equal(MemberType.Member, clanResponse.Result.Guild.MemberUids["888888888888888888888888888888"]);
            Assert.Equal(MemberType.Member, clanResponse.Result.Guild.MemberUids["999999999999999999999999999999"]);
            Assert.Equal(123, clanResponse.Result.Guild.CurrentNewRaidLevel);
            Assert.Equal(456789, clanResponse.Result.Guild.CurrentRaidLevel);

            Assert.Equal(10, clanResponse.Result.GuildMembers.Count);

            Assert.Equal("000000000000000000000000000000", clanResponse.Result.GuildMembers["0"].Uid);
            Assert.Equal(19375, clanResponse.Result.GuildMembers["0"].HighestZone);
            Assert.Equal("Member0", clanResponse.Result.GuildMembers["0"].Nickname);
            Assert.Equal(ClanClassType.Rogue, clanResponse.Result.GuildMembers["0"].ChosenClass);
            Assert.Equal(18, clanResponse.Result.GuildMembers["0"].ClassLevel);

            Assert.Equal("777777777777777777777777777777", clanResponse.Result.GuildMembers["1"].Uid);
            Assert.Equal(190569, clanResponse.Result.GuildMembers["1"].HighestZone);
            Assert.Equal("Member7", clanResponse.Result.GuildMembers["1"].Nickname);
            Assert.Equal(ClanClassType.Mage, clanResponse.Result.GuildMembers["1"].ChosenClass);
            Assert.Equal(32, clanResponse.Result.GuildMembers["1"].ClassLevel);

            Assert.Equal("666666666666666666666666666666", clanResponse.Result.GuildMembers["2"].Uid);
            Assert.Equal(1266039, clanResponse.Result.GuildMembers["2"].HighestZone);
            Assert.Equal("Member6", clanResponse.Result.GuildMembers["2"].Nickname);
            Assert.Equal(ClanClassType.Priest, clanResponse.Result.GuildMembers["2"].ChosenClass);
            Assert.Equal(25, clanResponse.Result.GuildMembers["2"].ClassLevel);

            Assert.Equal("888888888888888888888888888888", clanResponse.Result.GuildMembers["3"].Uid);
            Assert.Equal(18724, clanResponse.Result.GuildMembers["3"].HighestZone);
            Assert.Equal("Member8", clanResponse.Result.GuildMembers["3"].Nickname);
            Assert.Equal(ClanClassType.Priest, clanResponse.Result.GuildMembers["3"].ChosenClass);
            Assert.Equal(27, clanResponse.Result.GuildMembers["3"].ClassLevel);

            Assert.Equal("111111111111111111111111111111", clanResponse.Result.GuildMembers["4"].Uid);
            Assert.Equal(1115684, clanResponse.Result.GuildMembers["4"].HighestZone);
            Assert.Equal("Member1", clanResponse.Result.GuildMembers["4"].Nickname);
            Assert.Equal(ClanClassType.Mage, clanResponse.Result.GuildMembers["4"].ChosenClass);
            Assert.Equal(37, clanResponse.Result.GuildMembers["4"].ClassLevel);

            Assert.Equal("999999999999999999999999999999", clanResponse.Result.GuildMembers["5"].Uid);
            Assert.Equal(25184, clanResponse.Result.GuildMembers["5"].HighestZone);
            Assert.Equal("Member9", clanResponse.Result.GuildMembers["5"].Nickname);
            Assert.Equal(ClanClassType.Rogue, clanResponse.Result.GuildMembers["5"].ChosenClass);
            Assert.Equal(23, clanResponse.Result.GuildMembers["5"].ClassLevel);

            Assert.Equal("555555555555555555555555555555", clanResponse.Result.GuildMembers["6"].Uid);
            Assert.Equal(672309, clanResponse.Result.GuildMembers["6"].HighestZone);
            Assert.Equal("Member5", clanResponse.Result.GuildMembers["6"].Nickname);
            Assert.Equal(ClanClassType.Rogue, clanResponse.Result.GuildMembers["6"].ChosenClass);
            Assert.Equal(33, clanResponse.Result.GuildMembers["6"].ClassLevel);

            Assert.Equal("444444444444444444444444444444", clanResponse.Result.GuildMembers["7"].Uid);
            Assert.Equal(1640159, clanResponse.Result.GuildMembers["7"].HighestZone);
            Assert.Equal("Member4", clanResponse.Result.GuildMembers["7"].Nickname);
            Assert.Equal(ClanClassType.Rogue, clanResponse.Result.GuildMembers["7"].ChosenClass);
            Assert.Equal(19, clanResponse.Result.GuildMembers["7"].ClassLevel);

            Assert.Equal("222222222222222222222222222222", clanResponse.Result.GuildMembers["8"].Uid);
            Assert.Equal(2631014, clanResponse.Result.GuildMembers["8"].HighestZone);
            Assert.Equal("Member2", clanResponse.Result.GuildMembers["8"].Nickname);
            Assert.Equal(ClanClassType.Mage, clanResponse.Result.GuildMembers["8"].ChosenClass);
            Assert.Equal(37, clanResponse.Result.GuildMembers["8"].ClassLevel);

            Assert.Equal("333333333333333333333333333333", clanResponse.Result.GuildMembers["9"].Uid);
            Assert.Equal(3457914, clanResponse.Result.GuildMembers["9"].HighestZone);
            Assert.Equal("Member3", clanResponse.Result.GuildMembers["9"].Nickname);
            Assert.Equal(ClanClassType.Priest, clanResponse.Result.GuildMembers["9"].ChosenClass);
            Assert.Equal(37, clanResponse.Result.GuildMembers["9"].ClassLevel);
        }
    }
}
