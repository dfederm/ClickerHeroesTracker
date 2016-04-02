// <copyright file="Startup.Container.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System.Configuration;
    using System.Linq;
    using System.Web.Hosting;
    using System.Web.Http;
    using System.Web.Mvc;
    using Configuration;
    using Database;
    using Instrumentation;
    using Microsoft.ApplicationInsights;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.Mvc;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Models.Game;
    using Models.Settings;
    using UploadProcessing;
    using UnityLib = Microsoft.Practices.Unity;

    /// <summary>
    /// Configure the Unity container
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configure the Unity container
        /// </summary>
        /// <returns>The fully-configured Unity container</returns>
        private static IUnityContainer ConfigureContainer()
        {
            var container = new UnityContainer();

            // Container registrations
            RegisterTypes(container);

            // Inject filters using Unity
            FilterProviders.Providers.Remove(FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().First());
            FilterProviders.Providers.Add(new UnityFilterAttributeFilterProvider(container));

            // Use unity to resolve Mvc dependencies
            DependencyResolver.SetResolver(new UnityLib.Mvc.UnityDependencyResolver(container));

            // Use unity to resolve WebApi dependencies
            container.Resolve<HttpConfiguration>().DependencyResolver = new UnityLib.WebApi.UnityDependencyResolver(container);

            return container;
        }

        private static void RegisterTypes(UnityContainer container)
        {
            // Container controlled registrations
            container.RegisterType<CloudStorageAccount>(new ContainerControlledLifetimeManager(), new InjectionFactory(_ => CloudStorageAccount.Parse(ConfigurationManager.AppSettings.Get("StorageConnectionString"))));
            container.RegisterType<CloudTableClient>(new ContainerControlledLifetimeManager(), new InjectionFactory(_ => _.Resolve<CloudStorageAccount>().CreateCloudTableClient()));
            container.RegisterType<GameData>(new ContainerControlledLifetimeManager(), new InjectionFactory(_ => GameData.Parse(HostingEnvironment.MapPath("~\\App_Data\\GameData.json"))));
            container.RegisterType<HttpConfiguration>(new ContainerControlledLifetimeManager(), new InjectionFactory(_ => new HttpConfiguration()));
            container.RegisterType<IEnvironmentProvider, EnvironmentProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<IUploadProcessor, UploadProcessor>(new ContainerControlledLifetimeManager());
            container.RegisterType<IUploadScheduler, UploadScheduler>(new ContainerControlledLifetimeManager());

            // Per request registrations
            container.RegisterType<ICounterProvider, CounterProvider>(new PerRequestLifetimeManager());
            container.RegisterType<IDatabaseCommandFactory, DatabaseCommandFactory>(new PerRequestLifetimeManager());
            container.RegisterType<IUserSettingsProvider, UserSettingsProvider>(new PerRequestLifetimeManager());
            container.RegisterType<TelemetryClient>(new PerRequestLifetimeManager(), new InjectionFactory(_ => new TelemetryClient()));
        }
    }
}