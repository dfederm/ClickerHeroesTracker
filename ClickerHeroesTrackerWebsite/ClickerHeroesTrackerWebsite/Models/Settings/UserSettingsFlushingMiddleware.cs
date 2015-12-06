namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using Microsoft.Owin;
    using System.Threading.Tasks;

    public class UserSettingsFlushingMiddleware : OwinMiddleware
    {
        private readonly IUserSettingsProvider userSettingsProvider;

        public UserSettingsFlushingMiddleware(OwinMiddleware next, IUserSettingsProvider userSettingsProvider)
            : base(next)
        {
            this.userSettingsProvider = userSettingsProvider;
        }

        public async override Task Invoke(IOwinContext context)
        {
            await this.Next.Invoke(context);

            this.userSettingsProvider.FlushChanges();
        }
    }
}