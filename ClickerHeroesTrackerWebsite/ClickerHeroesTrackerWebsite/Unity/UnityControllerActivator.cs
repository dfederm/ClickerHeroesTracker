// <copyright file="UnityControllerActivator.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Unity
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Microsoft.Practices.Unity;

    public sealed class UnityControllerActivator : IControllerActivator
    {
        [SuppressMessage("Microsoft.Usage", "CA2213:Disposable fields should be disposed", Justification = "The object does not own the container")]
        private readonly IUnityContainer container;

        public UnityControllerActivator(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.container = container;
        }

        /// <inheritdoc/>
        public IController Create(RequestContext requestContext, Type controllerType)
        {
            return (IController)this.container.Resolve(controllerType);
        }
    }
}