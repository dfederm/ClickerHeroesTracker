// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using ClickerHeroesTrackerWebsite.Models.Game;

namespace ClickerHeroesTrackerWebsite.Tests.Mocks
{
    internal static class MockGameData
    {
        public static readonly GameData RealData = GameData.Parse("GameData.json");
    }
}
