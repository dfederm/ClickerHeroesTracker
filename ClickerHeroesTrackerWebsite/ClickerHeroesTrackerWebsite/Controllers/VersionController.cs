namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Configuration;
    using System.Net.Http;
    using System.Web.Http;

    [RoutePrefix("version")]
    [Authorize(Roles = "Admin")]
    public class VersionController : ApiController
    {
        private readonly IEnvironmentProvider environmentProvider;

        public VersionController(IEnvironmentProvider environmentProvider)
        {
            this.environmentProvider = environmentProvider;
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage Version()
        {
            var model = new
            {
                Environment = this.environmentProvider.Environment,
                Changelist = this.environmentProvider.Changelist,
                BuildId = this.environmentProvider.BuildId,
            };

            return this.Request.CreateResponse(model);
        }
    }
}
