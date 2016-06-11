// <copyright file="ComputedStatsModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Stats
{
    using System;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Models.Simulation;

    /// <summary>
    /// The model for the computed stats view.
    /// </summary>
    public class ComputedStatsModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedStatsModel"/> class.
        /// </summary>
        public ComputedStatsModel(
            GameData gameData,
            SavedGame savedGame,
            IUserSettings userSettings,
            ICounterProvider counterProvider)
        {
            // Simulation is only enabled for legacy saves, before outsiders were a thing.
            var simulationEnabled = savedGame.OutsidersData == null;
            if (simulationEnabled)
            {
                var simulation = new Simulation(
                    gameData,
                    savedGame,
                    userSettings.PlayStyle);

                Simulation.SimulateResult simulationResult;
                using (var scope = counterProvider.Measure(Counter.Simulation))
                {
                    simulationResult = simulation.Run();
                }

                this.SoulsPerHour = Convert.ToInt64(Math.Round(simulationResult.Ratio * 3600));
                this.OptimalLevel = simulationResult.Level;
                this.OptimalSoulsPerAscension = (int)Math.Round(simulationResult.Souls);
                this.OptimalAscensionTime = (short)Math.Min(
                    Math.Round(simulationResult.Time / 60),
                    short.MaxValue);
            }
        }

        /// <summary>
        /// Gets the optimal souls earned per hour
        /// </summary>
        public long SoulsPerHour { get; }

        /// <summary>
        /// Gets the optimal level to ascend.
        /// </summary>
        public int OptimalLevel { get; }

        /// <summary>
        /// Gets the expected souls earned per ascension if ascending at the optimal level.
        /// </summary>
        public long OptimalSoulsPerAscension { get; }

        /// <summary>
        /// Gets the optimal time it takes to reach the optimal level.
        /// </summary>
        public short OptimalAscensionTime { get; }
    }
}