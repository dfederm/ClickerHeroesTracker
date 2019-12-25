// <copyright file="VersionController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using ClickerHeroesTrackerWebsite.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Website.Models.Api.Version;

    /// <summary>
    /// A diagnostic controller for the service version.
    /// </summary>
    [Route("version")]
    [ApiController]
    public class VersionController : Controller
    {
        private readonly IBuildInfoProvider buildInfoProvider;

        private readonly IWebHostEnvironment webHostEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionController"/> class.
        /// </summary>
        public VersionController(
            IBuildInfoProvider buildInfoProvider,
            IWebHostEnvironment webHostEnvironment)
        {
            this.buildInfoProvider = buildInfoProvider;
            this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Displays debug info about the service version.
        /// </summary>
        /// <returns>An object with the version information.</returns>
        [Route("")]
        [HttpGet]
        public ActionResult<VersionResponse> Version() => new VersionResponse
        {
            Environment = this.webHostEnvironment.EnvironmentName,
            Changelist = this.buildInfoProvider.Changelist,
            BuildId = this.buildInfoProvider.BuildId,
            Webclient = this.buildInfoProvider.Webclient,
        };
    }
}
