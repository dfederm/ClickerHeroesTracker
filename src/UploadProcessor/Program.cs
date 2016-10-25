// <copyright file="Program.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTracker.UploadProcessor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.UploadProcessing;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// The entrypoint class to the program.
    /// </summary>
    public static class Program
    {
        private static ConcurrentBag<UploadProcessor> processors = new ConcurrentBag<UploadProcessor>();

        private static GameData gameData;

        private static TelemetryClient telemetryClient;

        private static IOptions<DatabaseSettings> databaseSettingsOptions;

        private static CloudQueueClient queueClient;

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

            gameData = GameData.Parse(@"GameData.json");

            // Set up configuration.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets()
                .AddApplicationInsightsSettings(developerMode: true);
            var configuration = builder.Build();

            var databaseSettings = new DatabaseSettings();
            configuration.GetSection("Database").Bind(databaseSettings);
            databaseSettingsOptions = Options.Create(databaseSettings);

            queueClient = CloudStorageAccount.Parse(configuration["Storage:ConnectionString"]).CreateCloudQueueClient();

            // Set up telemetry
            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];
            telemetryConfiguration.TelemetryProcessorChainBuilder.Use(next => new ConsoleLoggingProcessor(next)).Build();
            telemetryClient = new TelemetryClient(telemetryConfiguration);

            // Spin up one per logical core
            Console.WriteLine($"Number of processors: {Environment.ProcessorCount}");
            var tasks = new Task[Environment.ProcessorCount];
            var cancelSource = new CancellationTokenSource();
            for (var i = 0; i < tasks.Length; i++)
            {
                var processor = new UploadProcessor(databaseSettingsOptions, gameData, telemetryClient, queueClient);
                processors.Add(processor);
                tasks[i] = processor.ProcessAsync(cancelSource.Token);
            }

            // Let it run until Ctrl+C
            exitEvent.WaitOne();

            const int DrainTimeMs = 60000;
            Console.WriteLine($"Waiting for up to {DrainTimeMs}ms for processing to drain...");
            if (Task.WaitAll(tasks, DrainTimeMs))
            {
                Console.WriteLine($"Processing drained successfully");
            }
            else
            {
                Console.WriteLine($"Some processing was stuck");
                foreach (var processor in processors)
                {
                    var currentUploadId = processor.CurrentUploadId;
                    if (currentUploadId != null)
                    {
                        var properties = new Dictionary<string, string>
                        {
                            { "UploadId", currentUploadId.Value.ToString() }
                        };
                        telemetryClient.TrackEvent("UploadProcessor-Abandoned-Stuck", properties);
                    }
                }
            }

            telemetryClient.Flush();
        }

        // Taken from https://github.com/aspnet/Options/blob/dev/src/Microsoft.Extensions.Options/OptionsWrapper.cs
        // Remove after upgrading to RC2 and use Options.Create<TOptions>(TOptions options) instead.
        private sealed class OptionsWrapper<TOptions> : IOptions<TOptions> where TOptions : class, new()
        {
            public OptionsWrapper(TOptions options)
            {
                Value = options;
            }

            public TOptions Value { get; }
        }
    }
}
