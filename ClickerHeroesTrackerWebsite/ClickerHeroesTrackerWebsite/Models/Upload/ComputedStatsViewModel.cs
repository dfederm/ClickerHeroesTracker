namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using ClickerHeroesTrackerWebsite.Models.SaveData;
    using ClickerHeroesTrackerWebsite.Models.Simulation;
    using System;

    public class ComputedStatsViewModel
    {
        public ComputedStatsViewModel(SavedGame savedGame)
        {
            // No activities for now; assume idle mode
            var simulationResult = new Simulation(savedGame, null).Run();

            this.SoulsPerHour = Math.Round(simulationResult.Ratio * 3600);
            this.OptimalLevel = simulationResult.Level;
            this.OptimalSoulsPerAscension = Math.Round(simulationResult.Souls);
            this.OptimalAscensionTime = (int)Math.Round(simulationResult.Time / 60);
        }

        public double SoulsPerHour { get; private set; }

        public int OptimalLevel { get; private set; }

        public double OptimalSoulsPerAscension { get; private set; }

        public int OptimalAscensionTime { get; private set; }
    }
}