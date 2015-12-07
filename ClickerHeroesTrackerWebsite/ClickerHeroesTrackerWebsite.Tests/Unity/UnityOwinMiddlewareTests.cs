// <copyright file="UnityOwinMiddlewareTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Unity
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Unity;
    using Microsoft.Owin;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class UnityOwinMiddlewareTests
    {
        [TestMethod]
        public async Task UnityOwinMiddlewareTests_BasicTest()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<ContainerControlledDependency>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<TransientDependency>(new TransientLifetimeManager());

            var mockNextMiddleware = new Mock<OwinMiddleware>(MockBehavior.Strict, null);
            var mockOwinContext = new Mock<IOwinContext>(MockBehavior.Strict);

            var unityOwinMiddleware = new UnityOwinMiddleware<MockOwinMiddleware>(
                mockNextMiddleware.Object,
                unityContainer);

            // Simulate two parallel requests
            await Task.WhenAll(
                unityOwinMiddleware.Invoke(mockOwinContext.Object),
                unityOwinMiddleware.Invoke(mockOwinContext.Object));

            var mockOwinMiddlewares = MockOwinMiddleware.Instances;
            Assert.AreEqual(2, mockOwinMiddlewares.Count);
            Assert.AreNotEqual(mockOwinMiddlewares[0], mockOwinMiddlewares[1]);
            Assert.IsTrue(mockOwinMiddlewares[0].Completed);
            Assert.IsTrue(mockOwinMiddlewares[1].Completed);
            Assert.AreEqual(mockNextMiddleware.Object, mockOwinMiddlewares[0].Next);
            Assert.AreEqual(mockNextMiddleware.Object, mockOwinMiddlewares[1].Next);
            Assert.AreEqual(mockOwinMiddlewares[0].ContainerControlledDependency, mockOwinMiddlewares[1].ContainerControlledDependency);
            Assert.AreNotEqual(mockOwinMiddlewares[0].TransientDependency, mockOwinMiddlewares[1].TransientDependency);
        }

        private sealed class MockOwinMiddleware : OwinMiddleware
        {
            private static List<MockOwinMiddleware> instances = new List<MockOwinMiddleware>();

            public MockOwinMiddleware(
                OwinMiddleware next,
                ContainerControlledDependency containerControlledDependency,
                TransientDependency transientDependency)
                : base(next)
            {
                this.ContainerControlledDependency = containerControlledDependency;
                this.TransientDependency = transientDependency;

                // To track and assert
                Instances.Add(this);
            }

            public static List<MockOwinMiddleware> Instances
            {
                get
                {
                    return instances;
                }
            }

            public ContainerControlledDependency ContainerControlledDependency { get; private set; }

            public TransientDependency TransientDependency { get; private set; }

            public new OwinMiddleware Next
            {
                get
                {
                    return base.Next;
                }
            }

            public bool Completed { get; private set; }

            public async override Task Invoke(IOwinContext context)
            {
                await Task.Run(() => { this.Completed = true; });
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
