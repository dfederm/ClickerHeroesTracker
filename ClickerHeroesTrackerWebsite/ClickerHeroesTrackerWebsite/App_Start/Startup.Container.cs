// <copyright file="Startup.Container.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Mvc;
    using Configuration;
    using Database;
    using Instrumentation;
    using Microsoft.ApplicationInsights;
    using Microsoft.Practices.Unity;
    using Models.Settings;
    using Unity;

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

            // MVC Controllers should be created by Unity
            ControllerBuilder.Current.SetControllerFactory(new DefaultControllerFactory(new UnityControllerActivator(container)));

            // Web Api controllers should be created by Unity
            var httpConfiguration = container.Resolve<HttpConfiguration>();
            httpConfiguration.Services.Replace(typeof(IHttpControllerActivator), new UnityHttpControllerActivator(container));

            return container;
        }

        private static void RegisterTypes(UnityContainer container)
        {
            // Container controlled registrations
            container.RegisterType<HttpConfiguration>(new ContainerControlledLifetimeManager(), new InjectionFactory(_ => new HttpConfiguration()));
            container.RegisterType<IEnvironmentProvider, EnvironmentProvider>(new ContainerControlledLifetimeManager());

            // Owin Context (per request) registrations
            container.RegisterType<ICounterProvider, CounterProvider>(new OwinContextLifetimeManager());
            container.RegisterType<IDatabaseCommandFactory, DatabaseCommandFactory>(new OwinContextLifetimeManager());
            container.RegisterType<IUserSettingsProvider, UserSettingsProvider>(new OwinContextLifetimeManager());
            container.RegisterType<TelemetryClient>(new OwinContextLifetimeManager());
        }
    }
}