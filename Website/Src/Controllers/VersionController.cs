// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using ClickerHeroesTrackerWebsite.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Website.Models.Api.Version;

namespace ClickerHeroesTrackerWebsite.Controllers
{
    /// <summary>
    /// A diagnostic controller for the service version.
    /// </summary>
    [Route("version")]
    [ApiController]
    public class VersionController : Controller
    {
        private readonly IBuildInfoProvider _buildInfoProvider;

        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionController"/> class.
        /// </summary>
        public VersionController(
            IBuildInfoProvider buildInfoProvider,
            IWebHostEnvironment webHostEnvironment)
        {
            _buildInfoProvider = buildInfoProvider;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Displays debug info about the service version.
        /// </summary>
        /// <returns>An object with the version information.</returns>
        [Route("")]
        [HttpGet]
        public ActionResult<VersionResponse> Version() => new VersionResponse
        {
            Environment = _webHostEnvironment.EnvironmentName,
            Changelist = _buildInfoProvider.Changelist,
            BuildUrl = _buildInfoProvider.BuildUrl,
            Webclient = _buildInfoProvider.Webclient,
        };
    }
}
