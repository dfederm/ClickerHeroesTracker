// <copyright file="VersionController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System.Net.Http;
    using System.Web.Http;
    using Configuration;

    /// <summary>
    /// A diagnostic controller for the service version
    /// </summary>
    [RoutePrefix("version")]
    [Authorize(Roles = "Admin")]
    public class VersionController : ApiController
    {
        private readonly IEnvironmentProvider environmentProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionController"/> class.
        /// </summary>
        public VersionController(IEnvironmentProvider environmentProvider)
        {
            this.environmentProvider = environmentProvider;
        }

        /// <summary>
        /// Displays debug info about the service version
        /// </summary>
        /// <returns>An object with the version information</returns>
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
