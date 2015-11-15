namespace ClickerHeroesTrackerWebsite.Unity
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Microsoft.Practices.Unity;

    public sealed class UnityControllerFactory : DefaultControllerFactory
    {
        private readonly IUnityContainer container;

        public UnityControllerFactory(IUnityContainer container)
        {
            this.container = container;
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            return (IController)this.container.Resolve(controllerType);
        }
    }
}