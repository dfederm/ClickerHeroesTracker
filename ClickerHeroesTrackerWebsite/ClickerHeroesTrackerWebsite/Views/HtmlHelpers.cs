// <copyright file="HtmlHelpers.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Views
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Utility;

    public static class HtmlHelpers
    {
        public static IDisposable BeginScripts(this HtmlHelper helper)
        {
            return new ScriptBlock((WebViewPage)helper.ViewDataContainer);
        }

        public static MvcHtmlString PageScripts(this HtmlHelper helper)
        {
            return MvcHtmlString.Create(string.Join(Environment.NewLine, ScriptBlock.PageScripts.Select(s => s.ToString())));
        }

        /* Taken from: https://jadnb.wordpress.com/2011/02/16/rendering-scripts-from-partial-views-at-the-end-in-mvc/ */
        private class ScriptBlock : DisposableBase
        {
            private const string ScriptsKey = "scripts";

            private WebViewPage webPageBase;

            public ScriptBlock(WebViewPage webPageBase)
            {
                this.webPageBase = webPageBase;
                this.webPageBase.OutputStack.Push(new StringWriter());
            }

            public static List<string> PageScripts
            {
                get
                {
                    if (HttpContext.Current.Items[ScriptsKey] == null)
                    {
                        HttpContext.Current.Items[ScriptsKey] = new List<string>();
                    }

                    return (List<string>)HttpContext.Current.Items[ScriptsKey];
                }
            }

            protected override void Dispose(bool isDisposing)
            {
                PageScripts.Add(((StringWriter)this.webPageBase.OutputStack.Pop()).ToString());
            }
        }
    }
}