// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ClickerHeroesTrackerWebsite
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .Build()
                .Run();
        }
    }
}
