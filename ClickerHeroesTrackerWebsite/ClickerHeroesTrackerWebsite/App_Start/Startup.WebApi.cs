namespace ClickerHeroesTrackerWebsite
{
    using System;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using Newtonsoft.Json;
    using Owin;

    public partial class Startup
    {
        private static void ConfigureWebApi(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            // Replace the Json formatter with out own.
            config.Formatters.Remove(config.Formatters.JsonFormatter);
            config.Formatters.Add(new BrowserJsonFormatter());

            // Allow the json formatter to handle requests from the browser
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // Beautify by default for debuggability. When gzipping, this barely adds anything to the payload.
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;

            // Omit nulls
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            // Use attribute-based routing
            config.MapHttpAttributeRoutes();

            // Owin wireup
            app.UseWebApi(config);
        }

        /// <summary>
        /// This class behaves jsut like the default but forces a json content type
        /// </summary>
        private sealed class BrowserJsonFormatter : JsonMediaTypeFormatter
        {
            public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
            {
                base.SetDefaultContentHeaders(type, headers, mediaType);

                // Force the json content type so browsers can render it like json.
                headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
        }
    }
}