// <copyright file="Program.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTracker.UploadProcessor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Database;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.UploadProcessing;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// The entrypoint class to the program.
    /// </summary>
    public static class Program
    {
        private static ConcurrentBag<UploadProcessor> processors = new ConcurrentBag<UploadProcessor>();

        private static GameData gameData = GameData.Parse(@"GameData.json");

        private static TelemetryClient telemetryClient = new TelemetryClient();

        /// <summary>
        /// The entrypoint method to the program.
        /// </summary>
        /// <param name="args">The program arguments</param>
        public static void Main(string[] args)
        {
            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                // Cancel the exit, as we need to cleanup first.
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            // Spin up one per logical core
            Console.WriteLine($"Number of processors: {Environment.ProcessorCount}");
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                Task.Run(() => StartUploadProcessor());
            }

            // Let it run until Ctrl+C
            exitEvent.WaitOne();

            // Dispose of everything
            foreach (var processor in processors)
            {
                processor.Dispose();
            }

            const int DrainTimeMs = 60000;
            Console.WriteLine($"Waiting for {DrainTimeMs}ms for processing to drain...");
            Thread.Sleep(DrainTimeMs);

            telemetryClient.Flush();
        }

        private static void StartUploadProcessor()
        {
            var processor = new UploadProcessor(gameData, telemetryClient);
            processor.Start();
            processors.Add(processor);
        }
    }
}
