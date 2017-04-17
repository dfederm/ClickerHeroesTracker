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
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using ClickerHeroesTrackerWebsite.Services.Instrumentation;
    using ClickerHeroesTrackerWebsite.Services.UploadProcessing;
    using ClickerHeroesTrackerWebsite.UploadProcessing;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.WindowsAzure.Storage;
    using ClickerHeroesTrackerWebsite;

    /// <summary>
    /// The entrypoint class to the program.
    /// </summary>
    public static class Program
    {
        private const bool ScheduleRecompute = true;

        private const string LastComputeTime = "2017-03-27 03:04:18";

        /// <summary>
        /// The entrypoint method to the program.
        /// </summary>
        /// <param name="args">The program arguments</param>
        public static void Main(string[] args)
        {
            var exitEvent = new ManualResetEvent(false);
            var cancelSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                // Cancel the exit, as we need to cleanup first.
                eventArgs.Cancel = true;
                cancelSource.Cancel();
                exitEvent.Set();
            };

            var gameData = GameData.Parse(@"GameData.json");

            // Set up configuration.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Startup>()
                .AddApplicationInsightsSettings(developerMode: true);
            var configuration = builder.Build();

            var databaseSettings = new DatabaseSettings();
            configuration.GetSection("Database").Bind(databaseSettings);
            var databaseSettingsOptions = Options.Create(databaseSettings);

            var queueClient = CloudStorageAccount.Parse(configuration["Storage:ConnectionString"]).CreateCloudQueueClient();

            // Set up telemetry
            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];
            telemetryConfiguration.TelemetryProcessorChainBuilder.Use(next => new ConsoleLoggingProcessor(next)).Build();
            var telemetryClient = new TelemetryClient(telemetryConfiguration);
            var metricProvider = new MetricProvider(new MetricManager(telemetryClient));

            if (ScheduleRecompute)
            {
                Task.Run(async () =>
                {
                    var uploadIds = new List<int>();

                    const string CommandText = "SELECT Id FROM Uploads WHERE LastComputeTime < '" + LastComputeTime + "' ORDER BY LastComputeTime DESC";
                    using (var counterProvider = new CounterProvider(telemetryClient, metricProvider))
                    using (var databaseCommandFactory = new DatabaseCommandFactory(databaseSettingsOptions, counterProvider))
                    using (var command = databaseCommandFactory.Create(CommandText))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var uploadId = Convert.ToInt32(reader["Id"]);
                            uploadIds.Add(uploadId);
                        }
                    }

                    Console.WriteLine($"Found {uploadIds.Count} uploads to schedule");
                    using (var counterProvider = new CounterProvider(telemetryClient, metricProvider))
                    {
                        var uploadScheduler = new UploadScheduler(counterProvider, queueClient);
                        for (var i = 0; i < uploadIds.Count; i++)
                        {
                            if (cancelSource.IsCancellationRequested)
                            {
                                break;
                            }

                            Console.WriteLine($"Scheduling message {i + 1} of {uploadIds.Count} - {100 * (i + 1) / uploadIds.Count}%");
                            var message = new UploadProcessingMessage { UploadId = uploadIds[i], Requester = "Ad-hoc Recompute", Priority = UploadProcessingMessagePriority.Low };
                            await uploadScheduler.ScheduleAsync(message);
                        }
                    }

                    Console.WriteLine($"Stopped scheduling");
                });
            }

            var processors = new ConcurrentBag<UploadProcessor>();

            // Spin up one per logical core
            Console.WriteLine($"Number of processors: {Environment.ProcessorCount}");
            var tasks = new Task[Environment.ProcessorCount];
            for (var i = 0; i < tasks.Length; i++)
            {
                var processor = new UploadProcessor(databaseSettingsOptions, gameData, telemetryClient, metricProvider, queueClient);
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
    }
}
