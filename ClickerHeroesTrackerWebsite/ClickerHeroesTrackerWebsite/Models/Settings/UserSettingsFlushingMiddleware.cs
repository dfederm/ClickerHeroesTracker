// <copyright file="UserSettingsFlushingMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Settings
{
    using System.Threading.Tasks;
    using Microsoft.Owin;

    public class UserSettingsFlushingMiddleware : OwinMiddleware
    {
        private readonly IUserSettingsProvider userSettingsProvider;

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