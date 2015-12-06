namespace ClickerHeroesTrackerWebsite
{
    using System.Web.Http;
    using Microsoft.Practices.Unity;
    using Owin;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Unity;
    using Models.Settings;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = ConfigureContainer();

            // We want to start measuring latency as soon as possible during a request.
            app.Use<UnityOwinMiddleware<MeasureLatencyMiddleware>>(container);

            // Auth middleware. Needs to be added before any middleware that uses the user.
            ConfigureAuth(app);

            // Instrument the user as soon as they're auth'd.
            app.Use<UnityOwinMiddleware<UserInstrumentationMiddleware>>(container);

            // Flush any changes to user settings
            app.Use<UnityOwinMiddleware<UserSettingsFlushingMiddleware>>(container);

            // Routing middleware
            ConfigureWebApi(app, container.Resolve<HttpConfiguration>());
        }
    }
}
