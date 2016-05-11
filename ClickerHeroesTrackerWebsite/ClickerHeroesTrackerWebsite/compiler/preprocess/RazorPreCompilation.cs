// <copyright file="RazorPreCompilation.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using Microsoft.AspNet.Mvc.Razor.Precompilation;
    using Microsoft.Dnx.Compilation.CSharp;

    /// <summary>
    /// Precompiles all Razor views to ensure they don't have compile errors are runtime.
    /// Taken from example: https://github.com/aspnet/Mvc/blob/master/test/WebSites/PrecompilationWebSite/compiler/preprocess/RazorPreCompilation.cs
    /// </summary>
    public class RazorPreCompilation : RazorPreCompileModule
    {
        protected override bool EnablePreCompilation(BeforeCompileContext context) => true;
    }
}
