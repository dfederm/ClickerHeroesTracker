// <copyright file="ComputedStatsViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using System;
    using System.Data.SqlClient;
    using System.Linq;
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Simulation;
    using Game;
    using Settings;

    /// <summary>
    /// The model for the computed stats view.
    /// </summary>
    public class ComputedStatsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedStatsViewModel"/> class.
        /// </summary>
        public ComputedStatsViewModel(
            GameData gameData,
            SavedGame savedGame,
            IUserSettings userSettings)
        {
            this.UserSettings = userSettings;

            // No activities for now; assume idle mode
            var simulationResult = new Simulation(
                gameData,
                savedGame,
                null).Run();

            this.SoulsPerHour = Convert.ToInt64(Math.Round(simulationResult.Ratio * 3600));
            this.OptimalLevel = simulationResult.Level;
            this.OptimalSoulsPerAscension = (int)Math.Round(simulationResult.Souls);
            this.OptimalAscensionTime = (short)Math.Min(
                Math.Round(simulationResult.Time / 60),
                short.MaxValue);
            this.TitanDamage = savedGame.TitanDamage;
            this.SoulsSpent = savedGame.AncientsData.Ancients.Values.Aggregate(0L, (count, ancientData) => count + ancientData.SpentHeroSouls);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedStatsViewModel"/> class.
        /// </summary>
        public ComputedStatsViewModel(SqlDataReader reader, IUserSettings userSettings)
        {
            this.UserSettings = userSettings;

            if (reader.Read())
            {
                this.OptimalLevel = Convert.ToInt16(reader["OptimalLevel"]);
                this.SoulsPerHour = Convert.ToInt64(reader["SoulsPerHour"]);
                this.OptimalSoulsPerAscension = Convert.ToInt64(reader["SoulsPerAscension"]);
                this.OptimalAscensionTime = Convert.ToInt16(reader["AscensionTime"]);
                this.TitanDamage = Convert.ToInt64(reader["TitanDamage"]);
                this.SoulsSpent = Convert.ToInt64(reader["SoulsSpent"]);
            }
        }

        /// <summary>
        /// Gets the current user settings.
        /// </summary>
        public IUserSettings UserSettings { get; }

        /// <summary>
        /// Gets the optimal souls earned per hour
        /// </summary>
        public long SoulsPerHour { get; }

        /// <summary>
        /// Gets the optimal level to ascend.
        /// </summary>
        public short OptimalLevel { get; }

        /// <summary>
        /// Gets the expected souls earned per ascension if ascending at the optimal level.
        /// </summary>
        public long OptimalSoulsPerAscension { get; }

        /// <summary>
        /// Gets the optimal time it takes to reach the optimal level.
        /// </summary>
        public short OptimalAscensionTime { get; }

        /// <summary>
        /// Gets the user's titan damage
        /// </summary>
        public long TitanDamage { get; }

        /// <summary>
        /// Gets the number of souls the user has spent.
        /// </summary>
        public long SoulsSpent { get; }
    }
}