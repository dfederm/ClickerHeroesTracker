// <copyright file="UnityControllerActivatorTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Unity
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;
    using ClickerHeroesTrackerWebsite.Unity;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnityControllerActivatorTests
    {
        [TestMethod]
        public void UnityControllerActivator_Create_BasicTest()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<ContainerControlledDependency>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<TransientDependency>(new TransientLifetimeManager());

            var requestContext = new RequestContext();

            var unityControllerActivator = new UnityControllerActivator(unityContainer);

            // Validate a controller creation
            var controller1 = (MockController)unityControllerActivator.Create(requestContext, typeof(MockController));
            Assert.IsNotNull(controller1);
            Assert.IsNotNull(controller1.ContainerControlledDependency);
            Assert.IsNotNull(controller1.TransientDependency);

            // Validate another controller creation
            var controller2 = (MockController)unityControllerActivator.Create(requestContext, typeof(MockController));
            Assert.IsNotNull(controller2);
            Assert.IsNotNull(controller2.ContainerControlledDependency);
            Assert.IsNotNull(controller2.TransientDependency);

            // Validate the lifetimes between the two resolution
            Assert.AreNotEqual(controller1, controller2);
            Assert.AreEqual(controller1.ContainerControlledDependency, controller2.ContainerControlledDependency);
            Assert.AreNotEqual(controller1.TransientDependency, controller2.TransientDependency);
        }

        private sealed class MockController : IController
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

            public void Execute(RequestContext requestContext)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class ContainerControlledDependency
        {
        }

        private sealed class TransientDependency
        {
        }
    }
}
