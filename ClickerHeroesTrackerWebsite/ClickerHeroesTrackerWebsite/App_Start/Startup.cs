namespace ClickerHeroesTrackerWebsite
{
    using System.Web.Http;
    using Microsoft.Practices.Unity;
    using Owin;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Unity;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = ConfigureContainer(app);

            // Auth middleware
            ConfigureAuth(app);

            // Instrumentation middleware
            app.Use<UnityOwinMiddleware<UserInstrumentationMiddleware>>(container);

            // Routing middleware
            ConfigureWebApi(app, container.Resolve<HttpConfiguration>());
        }
    }
}
