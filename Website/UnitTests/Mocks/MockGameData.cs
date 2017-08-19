// <copyright file="MockGameData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Mocks
{
    using ClickerHeroesTrackerWebsite.Models.Game;

    internal static class MockGameData
    {
        public static readonly GameData RealData = GameData.Parse("GameData.json");
    }
}
