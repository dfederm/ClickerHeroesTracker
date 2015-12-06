namespace ClickerHeroesTrackerWebsite.Unity
{
    using Microsoft.Practices.Unity;
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;

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

        public IController Create(RequestContext requestContext, Type controllerType)
        {
            return (IController)this.container.Resolve(controllerType);
        }
    }
}