// <copyright file="IDatabaseCommandFactory.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Database
{
    using System.Collections.Generic;
    using System.Data;

    public interface IDatabaseCommandFactory
    {
        IDatabaseCommand Create(string commandText);

        IDatabaseCommand Create(string commandText, IDictionary<string, object> parameters);

        IDatabaseCommand Create(string commandText, CommandType commandType);

        IDatabaseCommand Create(string commandText, CommandType commandType, IDictionary<string, object> parameters);
    }
}
