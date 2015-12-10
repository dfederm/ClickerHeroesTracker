// <copyright file="UnityHttpControllerActivator.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Unity
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using Microsoft.Practices.Unity;

    public sealed class UnityHttpControllerActivator : IHttpControllerActivator
    {
        [SuppressMessage("Microsoft.Usage", "CA2213:Disposable fields should be disposed", Justification = "The object does not own the container")]
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