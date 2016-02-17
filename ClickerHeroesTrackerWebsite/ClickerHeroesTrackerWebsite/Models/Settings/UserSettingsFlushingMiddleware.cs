// <copyright file="UserSettingsFlushingMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using System.Threading.Tasks;
    using Microsoft.Owin;

    /// <summary>
    /// An <see cref="OwinMiddleware"/> which flushes the current user's settings at the end of the request.
    /// </summary>
    public class UserSettingsFlushingMiddleware : OwinMiddleware
    {
        private readonly IUserSettingsProvider userSettingsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettingsFlushingMiddleware"/> class.
        /// </summary>
        public UserSettingsFlushingMiddleware(OwinMiddleware next, IUserSettingsProvider userSettingsProvider)
            : base(next)
        {
            this.userSettingsProvider = userSettingsProvider;
        }

        /// <inheritdoc/>
        public async override Task Invoke(IOwinContext context)
        {
            await this.Next.Invoke(context);

            this.userSettingsProvider.FlushChanges();
        }
    }
}