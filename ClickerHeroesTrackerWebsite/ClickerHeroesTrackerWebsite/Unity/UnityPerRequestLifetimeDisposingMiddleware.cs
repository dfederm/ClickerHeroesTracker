// <copyright file="UnityPerRequestLifetimeDisposingMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Owin;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.Mvc;

    /// <summary>
    /// A replacement for Unity.Mvc's <see cref="UnityPerRequestHttpModule"/>, which disposes objects too soon.
    /// This middleware emulates the behavior, but does it in the Owin pipeline.
    /// </summary>
    public class UnityPerRequestLifetimeDisposingMiddleware : OwinMiddleware
    {
        // This is a terrible hack, but the most correct way to emulate UnityPerRequestHttpModule
        private static readonly object Key = typeof(UnityPerRequestHttpModule).GetField("ModuleKey", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityPerRequestLifetimeDisposingMiddleware"/> class.
        /// </summary>
        public UnityPerRequestLifetimeDisposingMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        /// <inheritdoc />
        public override async Task Invoke(IOwinContext context)
        {
            await this.Next.Invoke(context);

            // Based on logic in UnityPerRequestHttpModule
            var cache = (IDictionary<object, object>)HttpContext.Current.Items[Key];
            if (cache != null)
            {
                foreach (var disposable in cache.Values.OfType<IDisposable>())
                {
                    disposable.Dispose();
                }
            }
        }
    }
}