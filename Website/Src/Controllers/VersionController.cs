// <copyright file="VersionController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A diagnostic controller for the service version
    /// </summary>
    [Route("version")]
    public class VersionController : Controller
    {
        private readonly IBuildInfoProvider buildInfoProvider;

        private readonly IHostingEnvironment hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionController"/> class.
        /// </summary>
        public VersionController(
            IBuildInfoProvider buildInfoProvider,
            IHostingEnvironment hostingEnvironment)
        {
            this.buildInfoProvider = buildInfoProvider;
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Displays debug info about the service version
        /// </summary>
        /// <returns>An object with the version information</returns>
        [Route("")]
        [HttpGet]
        public IActionResult Version()
        {
            var model = new
            {
                Environment = this.hostingEnvironment.EnvironmentName,
                Changelist = this.buildInfoProvider.Changelist,
                BuildId = this.buildInfoProvider.BuildId,
                Webclient = this.buildInfoProvider.Webclient,
            };

            return this.Ok(model);
        }
    }
}
