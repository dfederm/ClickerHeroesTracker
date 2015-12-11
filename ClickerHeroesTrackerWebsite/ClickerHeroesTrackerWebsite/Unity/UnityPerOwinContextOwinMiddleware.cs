// <copyright file="UnityPerOwinContextOwinMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Owin;

    public sealed class UnityPerOwinContextOwinMiddleware : OwinMiddleware
    {
        private static readonly string Key = "UnityLifetimeCache";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityPerOwinContextOwinMiddleware"/> class.
        /// </summary>
        public UnityPerOwinContextOwinMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            await this.Next.Invoke(context);

            // Dispose all cached objects
            var dict = GetUnityLifetimeCache(context, false);
            if (dict != null)
            {
                foreach (var disposable in dict.Values.OfType<IDisposable>())
                {
                    disposable.Dispose();
                }
            }
        }

        internal static IDictionary<object, object> GetUnityLifetimeCache()
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                return null;
            }

            return GetUnityLifetimeCache(httpContext.GetOwinContext(), true);
        }

        private static IDictionary<object, object> GetUnityLifetimeCache(IOwinContext context, bool createIfNull)
        {
            if (context == null)
            {
                return null;
            }

            Dictionary<object, object> cache = null;
            object rawCache;
            if ((!context.Environment.TryGetValue(Key, out rawCache) || (cache = rawCache as Dictionary<object, object>) == null)
                && createIfNull)
            {
                context.Environment[Key] = cache = new Dictionary<object, object>();
            }

            return cache;
        }
    }
}