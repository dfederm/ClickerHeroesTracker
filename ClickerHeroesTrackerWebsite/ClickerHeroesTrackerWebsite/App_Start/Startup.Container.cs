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

    public partial class Startup
    {
        private static IUnityContainer ConfigureContainer(IAppBuilder app)
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

            // Call context (per request) registrations
            container.RegisterType<IDatabaseCommandFactory, DatabaseCommandProvider>(new CallContextLifetimeManager());
            container.RegisterType<TelemetryClient>(new CallContextLifetimeManager());
        }
    }
}