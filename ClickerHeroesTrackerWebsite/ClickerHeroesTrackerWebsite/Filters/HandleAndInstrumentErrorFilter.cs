// <copyright file="HandleAndInstrumentErrorFilter.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Filters
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using Models;

    /// <summary>
    /// An exception filter which handles and logs exceptions. Based on <see cref="HandleErrorAttribute"/>.
    /// </summary>
    public sealed class HandleAndInstrumentErrorFilter : IExceptionFilter
    {
        /// <inheritdoc/>
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.IsChildAction)
            {
                return;
            }

            // If custom errors are disabled, we need to let the normal ASP.NET exception handler
            // execute so that the user can see useful debugging information.
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            var exception = filterContext.Exception;

            // If this is not an HTTP 500 (for example, if somebody throws an HTTP 404 from an action method), ignore it.
            if (new HttpException(null, exception).GetHttpCode() != 500)
            {
                return;
            }

            // Instrument
            Telemetry.Client.TrackException(exception);

            // Return the error view
            var model = new HandleErrorInfo(
                exception,
                (string)filterContext.RouteData.Values["controller"],
                (string)filterContext.RouteData.Values["action"]);
            filterContext.Result = new ViewResult
            {
                ViewName = "Error",
                MasterName = string.Empty,
                ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                TempData = filterContext.Controller.TempData
            };
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            // Certain versions of IIS will sometimes use their own error page when
            // they detect a server error. Setting this property indicates that we
            // want it to try to render ASP.NET MVC's error page instead.
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}