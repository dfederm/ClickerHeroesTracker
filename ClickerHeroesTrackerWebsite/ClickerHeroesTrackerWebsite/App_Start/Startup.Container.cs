namespace ClickerHeroesTrackerWebsite
{
    using System.Web.Mvc;
    using Unity;
    using Microsoft.Practices.Unity;
    using Owin;
    using Configuration;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using Database;
    using Microsoft.ApplicationInsights;
    using Instrumentation;
    using Models.Settings;

    public partial class Startup
    {
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
            container.RegisterType<IDatabaseCommandFactory, DatabaseCommandProvider>(new OwinContextLifetimeManager());
            container.RegisterType<IUserSettingsProvider, UserSettingsProvider>(new OwinContextLifetimeManager());
            container.RegisterType<TelemetryClient>(new OwinContextLifetimeManager());
        }
    }
}