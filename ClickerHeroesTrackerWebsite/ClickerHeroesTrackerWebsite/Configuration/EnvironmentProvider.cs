namespace ClickerHeroesTrackerWebsite.Configuration
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Web.Hosting;

    public sealed class EnvironmentProvider : IEnvironmentProvider
    {
        public EnvironmentProvider()
        {
            // This is a slot setting set in the Azure portal.
            this.Environment = ConfigurationManager.AppSettings.Get("Environment") ?? "Devmachine";

            var buildInfoFile = HostingEnvironment.MapPath(@"~\App_Data\BuildInfo.json");
            if (!File.Exists(buildInfoFile))
            {
                throw new InvalidDataException("Could not find build info file: " + buildInfoFile);
            }

            // PreBuild.ps1 writes over this file during cloud build.
            using (var reader = new JsonTextReader(new StreamReader(buildInfoFile)))
            {
                var buildInfo = JObject.Load(reader);

                this.Changelist = (int)buildInfo["changelist"];
                this.BuildId = (string)buildInfo["buildId"];
            }
        }

        public string Environment { get; private set; }

        public int Changelist { get; private set; }

        public string BuildId { get; private set; }
    }
}