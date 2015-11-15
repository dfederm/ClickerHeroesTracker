using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClickerHeroesTrackerWebsite.Controllers;

namespace ClickerHeroesTrackerWebsite.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
