using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ClickerHeroesTrackerWebsite.Startup))]
namespace ClickerHeroesTrackerWebsite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureContainer(app);
            ConfigureAuth(app);
            ConfigureWebApi(app);
        }
    }
}
