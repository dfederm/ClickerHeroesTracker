namespace ClickerHeroesTrackerWebsite.Configuration
{
    using System.Configuration;
    using System.Reflection;

    public sealed class EnvironmentProvider : IEnvironmentProvider
    {
        public EnvironmentProvider()
        {
            // This is a slot setting set in the Azure portal.
            this.Environment = ConfigurationManager.AppSettings.Get("Environment") ?? "Devmachine";

            // PreBuild.ps1 sets this during cloud build.
            this.Changelist = Assembly.GetExecutingAssembly().GetName().Version.Build;
        }

        public string Environment { get; private set; }

        public int Changelist { get; private set; }
    }
}