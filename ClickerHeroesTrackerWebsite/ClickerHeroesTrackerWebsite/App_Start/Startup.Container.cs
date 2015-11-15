namespace ClickerHeroesTrackerWebsite
{
    using System.Web.Mvc;
    using Unity;
    using Microsoft.Practices.Unity;
    using Owin;

    public partial class Startup
    {
        private static void ConfigureContainer(IAppBuilder app)
        {
            var container = new UnityContainer();

            // Controllers should be created by Unity
            ControllerBuilder.Current.SetControllerFactory(new UnityControllerFactory(container));
        }
    }
}