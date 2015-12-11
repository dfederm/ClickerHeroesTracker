// <copyright file="OwinContextLifetimeManager.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using Microsoft.Practices.Unity;

    public class OwinContextLifetimeManager : LifetimeManager
    {
        private readonly object key = new object();

        /// <inheritdoc/>
        public override object GetValue()
        {
            var environment = UnityPerOwinContextOwinMiddleware.GetUnityLifetimeCache();
            if (environment == null)
            {
                return null;
            }

            object value;
            return environment.TryGetValue(this.key, out value) ? value : null;
        }

        /// <inheritdoc/>
        public override void SetValue(object newValue)
        {
            var environment = UnityPerOwinContextOwinMiddleware.GetUnityLifetimeCache();
            if (environment == null)
            {
                return;
            }

            environment.Add(this.key, newValue);
        }

        /// <inheritdoc/>
        public override void RemoveValue()
        {
            var environment = UnityPerOwinContextOwinMiddleware.GetUnityLifetimeCache();
            if (environment == null)
            {
                return;
            }

            object value;
            if (environment.TryGetValue(this.key, out value))
            {
                var disposableValue = value as IDisposable;
                if (disposableValue != null)
                {
                    disposableValue.Dispose();
                }

                environment.Remove(this.key);
            }
        }
    }
}