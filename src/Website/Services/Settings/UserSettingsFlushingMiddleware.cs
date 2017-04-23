// <copyright file="UserSettingsFlushingMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A middleware which flushes the current user's settings at the end of the request.
    /// </summary>
    public class UserSettingsFlushingMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettingsFlushingMiddleware"/> class.
        /// </summary>
        public UserSettingsFlushingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Executes this middleware
        /// </summary>
        /// <param name="context">The current http context</param>
        /// <returns>The async task</returns>
        public async Task Invoke(HttpContext context)
        {
            await this.next.Invoke(context);

            var userSettingsProvider = (IUserSettingsProvider)context.RequestServices.GetService(typeof(IUserSettingsProvider));
            userSettingsProvider.FlushChanges();
        }
    }
}