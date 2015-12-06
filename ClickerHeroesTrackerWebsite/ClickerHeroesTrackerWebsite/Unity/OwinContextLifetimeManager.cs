namespace ClickerHeroesTrackerWebsite.Unity
{
    using Microsoft.Practices.Unity;
    using System;
    using System.Collections.Generic;
    using System.Web;

    public class OwinContextLifetimeManager: LifetimeManager
    {
        private readonly string key = Guid.NewGuid().ToString();

        public override object GetValue()
        {
            var environment = GetOwinEnvironment();
            if (environment == null)
            {
                return null;
            }

            object value;
            return environment.TryGetValue(this.key, out value) ? value : null;
        }

        public override void SetValue(object newValue)
        {
            var environment = GetOwinEnvironment();
            if (environment == null)
            {
                return;
            }

            environment.Add(this.key, newValue);
        }

        public override void RemoveValue()
        {
            var environment = GetOwinEnvironment();
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

        private static IDictionary<string, object> GetOwinEnvironment()
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                return null;
            }

            return httpContext.GetOwinContext().Environment;
        }
    }
}