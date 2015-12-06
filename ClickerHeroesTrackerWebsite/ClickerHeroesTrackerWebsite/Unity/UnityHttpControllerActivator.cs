// <copyright file="UnityHttpControllerActivator.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Unity
{
    using System;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using Microsoft.Practices.Unity;

    public sealed class UnityHttpControllerActivator : IHttpControllerActivator
    {
        private IUnityContainer container;

        public UnityHttpControllerActivator(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.container = container;
        }

        /// <inheritdoc/>
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return (IHttpController)this.container.Resolve(controllerType);
        }
    }
}