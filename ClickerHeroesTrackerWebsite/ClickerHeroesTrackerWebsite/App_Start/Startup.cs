using System.Web.Http;
using ClickerHeroesTrackerWebsite;
using Microsoft.Owin;
using Microsoft.Practices.Unity;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace ClickerHeroesTrackerWebsite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = ConfigureContainer(app);
            ConfigureAuth(app);
            ConfigureWebApi(app, container.Resolve<HttpConfiguration>());
        }
    }
}
