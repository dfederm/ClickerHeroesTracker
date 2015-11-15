namespace ClickerHeroesTrackerWebsite.IntegrationTests.Helpers
{
    using System.IO;
    using System.Net;

    internal sealed class HtmlResponse
    {
        private readonly HttpWebResponse response;

        private string content;

        public HtmlResponse(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                this.response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                this.response = (HttpWebResponse)e.Response;
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return this.response.StatusCode;
            }
        }

        public string Content
        {
            get
            {
                return this.content = (this.content = new StreamReader(this.response.GetResponseStream()).ReadToEnd());
            }
        }
    }
}
