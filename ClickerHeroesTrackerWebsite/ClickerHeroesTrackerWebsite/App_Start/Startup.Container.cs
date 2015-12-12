// <copyright file="Startup.Container.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System.Linq;
    using System.Web.Http;
    using System.Web.Mvc;
    using Configuration;
    using Database;
    using Instrumentation;
    using Microsoft.ApplicationInsights;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.Mvc;
    using Models.Settings;

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

            // Mvc and Web Api Controllers should be created using Unity
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            return container;
        }

        private static void RegisterTypes(UnityContainer container)
        {
            // Container controlled registrations
            container.RegisterType<HttpConfiguration>(new ContainerControlledLifetimeManager(), new InjectionFactory(_ => new HttpConfiguration()));
            container.RegisterType<IEnvironmentProvider, EnvironmentProvider>(new ContainerControlledLifetimeManager());

            // Per request registrations
            container.RegisterType<ICounterProvider, CounterProvider>(new PerRequestLifetimeManager());
            container.RegisterType<IDatabaseCommandFactory, DatabaseCommandFactory>(new PerRequestLifetimeManager());
            container.RegisterType<IUserSettingsProvider, UserSettingsProvider>(new PerRequestLifetimeManager());
            container.RegisterType<TelemetryClient>(new PerRequestLifetimeManager());
        }
    }
}