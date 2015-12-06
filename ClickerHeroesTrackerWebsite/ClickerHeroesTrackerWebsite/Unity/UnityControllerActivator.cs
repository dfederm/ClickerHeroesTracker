// <copyright file="UnityControllerActivator.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Unity
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Microsoft.Practices.Unity;

    public sealed class UnityControllerActivator : IControllerActivator
    {
        private IUnityContainer container;

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