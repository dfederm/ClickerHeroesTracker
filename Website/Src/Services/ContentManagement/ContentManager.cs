// <copyright file="ContentManager.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.ContentManagement
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Hosting;
    using Utility;

    public sealed class ContentManager : IContentManager
    {
        private const string ScriptPath = "/js/";

        private readonly List<string> scripts = new List<string>();

        private readonly List<string> rawScripts = new List<string>();

        private readonly string scriptExtension;

        public ContentManager(IHostingEnvironment environment)
        {
            this.scriptExtension = environment.IsDevelopment() || environment.IsBuddy() ? ".js" : ".min.js";
        }

        public IEnumerable<string> Scripts => this.scripts;

        public IEnumerable<string> RawScripts => this.rawScripts;

        public void RegisterScript(string name)
        {
            if (name.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("//", StringComparison.OrdinalIgnoreCase))
            {
                this.scripts.Add(name);
            }
            else
            {
                this.scripts.Add(ScriptPath + name + this.scriptExtension);
            }
        }

        public void RegisterRawScript(string content)
        {
            this.rawScripts.Add(content);
        }
    }
}
