// <copyright file="HandleAndInstrumentErrorFilter.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Filters
{
    using System.Web.Mvc;
    using Models;

    public class HandleAndInstrumentErrorFilter : HandleErrorAttribute
    {
        /// <inheritdoc/>
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)
            {
                if (filterContext.HttpContext.IsCustomErrorEnabled)
                {
                    Telemetry.Client.TrackException(filterContext.Exception);
                }
            }

            base.OnException(filterContext);
        }
    }
}