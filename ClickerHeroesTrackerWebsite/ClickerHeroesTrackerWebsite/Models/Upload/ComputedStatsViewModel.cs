namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Simulation;
    using System;
    using System.Data.SqlClient;

    public class ComputedStatsViewModel
    {
        public ComputedStatsViewModel(SavedGame savedGame)
        {
            // No activities for now; assume idle mode
            var simulationResult = new Simulation(savedGame, null).Run();

            this.SoulsPerHour = Math.Round(simulationResult.Ratio * 3600);
            this.OptimalLevel = simulationResult.Level;
            this.OptimalSoulsPerAscension = (int)Math.Round(simulationResult.Souls);
            this.OptimalAscensionTime = (short)Math.Min(
                Math.Round(simulationResult.Time / 60),
                short.MaxValue);
        }

        public ComputedStatsViewModel(SqlDataReader reader)
        {
            if (reader.Read())
            {
                this.OptimalLevel = (short)reader["OptimalLevel"];
                this.SoulsPerHour = (int)reader["SoulsPerHour"];
                this.OptimalSoulsPerAscension = (int)reader["SoulsPerAscension"];
                this.OptimalAscensionTime = (short)reader["AscensionTime"];
            }
        }

        public double SoulsPerHour { get; private set; }

        public int OptimalLevel { get; private set; }

        public int OptimalSoulsPerAscension { get; private set; }

        public short OptimalAscensionTime { get; private set; }
    }
}