namespace ClickerHeroesTrackerWebsite.Unity
{
    using Microsoft.Practices.Unity;
    using System;
    using System.Web.Http.Dispatcher;
    using System.Net.Http;
    using System.Web.Http.Controllers;

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

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return (IHttpController)this.container.Resolve(controllerType);
        }
    }
}