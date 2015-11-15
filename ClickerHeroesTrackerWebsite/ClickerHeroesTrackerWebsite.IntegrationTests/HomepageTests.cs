namespace ClickerHeroesTrackerWebsite.IntegrationTests
{
    using System.Net;
    using Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HomepageTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Homepage_BasicTest()
        {
            var response = new HtmlResponse("/");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
