// <copyright file="HtmlHelpers.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Views
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using Utility;

    /// <summary>
    /// <see cref="HtmlHelper"/> extensions to be used in Razor views.
    /// </summary>
    public static class HtmlHelpers
    {
        /// <summary>
        /// Begins a block which should be buffered to the common script block.
        /// </summary>
        /// <remarks>
        /// <see cref="PageScripts(HtmlHelper)"/> must be called sometime after all calls to this method.
        /// </remarks>
        /// <returns>A block that scopes the operation</returns>
        public static IDisposable BeginScripts(this HtmlHelper helper)
        {
            return new ScriptBlock(
                helper.ViewContext.HttpContext,
                (WebViewPage)helper.ViewDataContainer);
        }

        /// <summary>
        /// Writes all content previously deferred using <see cref="BeginScripts(HtmlHelper)"/>.
        /// </summary>
        /// <returns>A string of the deferred content</returns>
        public static MvcHtmlString PageScripts(this HtmlHelper helper)
        {
            var writer = ScriptBlock.GetWriter(helper.ViewContext.HttpContext);
            return MvcHtmlString.Create(writer.ToString());
        }

        // Taken from: https://jadnb.wordpress.com/2011/02/16/rendering-scripts-from-partial-views-at-the-end-in-mvc/
        private class ScriptBlock : DisposableBase
        {
            private const string ScriptsKey = "scripts";

            private readonly WebViewPage webPageBase;

            public ScriptBlock(HttpContextBase httpContext, WebViewPage webPageBase)
            {
                this.webPageBase = webPageBase;
                this.webPageBase.OutputStack.Push(GetWriter(httpContext));
            }

            public static StringWriter GetWriter(HttpContextBase httpContext)
            {
                var writer = httpContext.Items[ScriptsKey] as StringWriter;
                if (writer == null)
                {
                    httpContext.Items[ScriptsKey] = writer = new StringWriter();
                }

                return writer;
            }

            protected override void Dispose(bool isDisposing)
            {
                this.webPageBase.OutputStack.Pop();
            }
        }
    }
}