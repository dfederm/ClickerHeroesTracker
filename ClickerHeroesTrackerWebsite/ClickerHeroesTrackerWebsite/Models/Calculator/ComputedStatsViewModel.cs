namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Simulation;
    using System;
    using System.Data.SqlClient;
    using System.Linq;

    public class ComputedStatsViewModel
    {
        public ComputedStatsViewModel(SavedGame savedGame, UserSettings userSettings)
        {
            this.UserSettings = userSettings;

            // No activities for now; assume idle mode
            var simulationResult = new Simulation(savedGame, null).Run();

            this.SoulsPerHour = Math.Round(simulationResult.Ratio * 3600);
            this.OptimalLevel = simulationResult.Level;
            this.OptimalSoulsPerAscension = (int)Math.Round(simulationResult.Souls);
            this.OptimalAscensionTime = (short)Math.Min(
                Math.Round(simulationResult.Time / 60),
                short.MaxValue);
            this.TitanDamage = savedGame.TitanDamage;
            this.SoulsSpent = savedGame.AncientsData.Ancients.Values.Aggregate(0, (count, ancientData) => count + ancientData.SpentHeroSouls);
        }

        public ComputedStatsViewModel(SqlDataReader reader, UserSettings userSettings)
        {
            this.UserSettings = userSettings;

            if (reader.Read())
            {
                this.OptimalLevel = (short)reader["OptimalLevel"];
                this.SoulsPerHour = (int)reader["SoulsPerHour"];
                this.OptimalSoulsPerAscension = (int)reader["SoulsPerAscension"];
                this.OptimalAscensionTime = (short)reader["AscensionTime"];
                this.TitanDamage = (int)reader["TitanDamage"];
                this.SoulsSpent = (int)reader["SoulsSpent"];
            }
        }

        public UserSettings UserSettings { get; private set; }

        public double SoulsPerHour { get; private set; }

        public int OptimalLevel { get; private set; }

        public int OptimalSoulsPerAscension { get; private set; }

        public short OptimalAscensionTime { get; private set; }

        public int TitanDamage { get; private set; }

        public int SoulsSpent { get; private set; }
    }
}