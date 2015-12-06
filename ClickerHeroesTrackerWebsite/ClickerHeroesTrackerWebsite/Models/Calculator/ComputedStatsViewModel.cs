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
    using Settings;

    public class ComputedStatsViewModel
    {
        public ComputedStatsViewModel(SavedGame savedGame, IUserSettings userSettings)
        {
            this.UserSettings = userSettings;

            // No activities for now; assume idle mode
            var simulationResult = new Simulation(savedGame, null).Run();

            this.SoulsPerHour = Convert.ToInt64(Math.Round(simulationResult.Ratio * 3600));
            this.OptimalLevel = simulationResult.Level;
            this.OptimalSoulsPerAscension = (int)Math.Round(simulationResult.Souls);
            this.OptimalAscensionTime = (short)Math.Min(
                Math.Round(simulationResult.Time / 60),
                short.MaxValue);
            this.TitanDamage = savedGame.TitanDamage;
            this.SoulsSpent = savedGame.AncientsData.Ancients.Values.Aggregate(0L, (count, ancientData) => count + ancientData.SpentHeroSouls);
        }

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

        public IUserSettings UserSettings { get; private set; }

        public long SoulsPerHour { get; private set; }

        public short OptimalLevel { get; private set; }

        public long OptimalSoulsPerAscension { get; private set; }

        public short OptimalAscensionTime { get; private set; }

        public long TitanDamage { get; private set; }

        public long SoulsSpent { get; private set; }
    }
}