namespace ClickerHeroesTrackerWebsite.Tests.Unity
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc;
    using System.Web.Routing;
    using ClickerHeroesTrackerWebsite.Unity;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class UnityControllerFactoryTests
    {
        [TestMethod]
        public void UnityControllerFactory_GetControllerInstance_BasicTest()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<ContainerControlledDependency>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<TransientDependency>(new TransientLifetimeManager());

            var requestContext = new RequestContext();

            var unityControllerFactory = new UnityControllerFactory(unityContainer);

            // Use reflection to get GetControllerInstance. It's protected and the
            // public method CreateController requires a bunch of MVC setup which is
            // seemingly impossible to do in a unit test.
            var getControllerInstanceMethod = (typeof(UnityControllerFactory)).GetMethod("GetControllerInstance", BindingFlags.Instance | BindingFlags.NonPublic);
            Func<MockController> getControllerInstance = () =>
            {
                return (MockController)getControllerInstanceMethod.Invoke(
                    unityControllerFactory,
                    new object[] { requestContext, typeof(MockController) });
            };

            // Validate a controller creation
            var controller1 = getControllerInstance();
            Assert.IsNotNull(controller1);
            Assert.IsNotNull(controller1.ContainerControlledDependency);
            Assert.IsNotNull(controller1.TransientDependency);

            // Validate another controller creation
            var controller2 = getControllerInstance();
            Assert.IsNotNull(controller2);
            Assert.IsNotNull(controller2.ContainerControlledDependency);
            Assert.IsNotNull(controller2.TransientDependency);

            // Validate the lifetimes between the two resolution
            Assert.AreEqual(controller1.ContainerControlledDependency, controller2.ContainerControlledDependency);
            Assert.AreNotEqual(controller1.TransientDependency, controller2.TransientDependency);
        }

        private sealed class MockController : Controller
        {
            public MockController(
                ContainerControlledDependency containerControlledDependency,
                TransientDependency transientDependency)
            {
                this.ContainerControlledDependency = containerControlledDependency;
                this.TransientDependency = transientDependency;
            }

            public ContainerControlledDependency ContainerControlledDependency { get; private set; }

            public TransientDependency TransientDependency { get; private set; }
        }

        private sealed class ContainerControlledDependency
        {
        }

        private sealed class TransientDependency
        {
        }
    }
}
