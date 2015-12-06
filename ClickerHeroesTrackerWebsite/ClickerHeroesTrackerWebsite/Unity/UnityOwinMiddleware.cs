// <copyright file="UnityOwinMiddleware.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Unity
{
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Microsoft.Practices.Unity;

    public sealed class UnityOwinMiddleware<T> : OwinMiddleware where T : OwinMiddleware
    {
        private readonly IUnityContainer container;

        public UnityOwinMiddleware(OwinMiddleware next, IUnityContainer container)
            : base(next)
        {
            this.container = container;
        }

        /// <inheritdoc/>
        public override Task Invoke(IOwinContext context)
        {
            return this.container.Resolve<T>(new ParameterOverride("next", this.Next)).Invoke(context);
        }
    }
}