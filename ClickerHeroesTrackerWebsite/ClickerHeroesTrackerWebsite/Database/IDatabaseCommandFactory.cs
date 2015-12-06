using System.Collections.Generic;
using System.Data;

namespace ClickerHeroesTrackerWebsite.Database
{
    public interface IDatabaseCommandFactory
    {
        IDatabaseCommand Create(string commandText);

        IDatabaseCommand Create(string commandText, IDictionary<string, object> parameters);

        IDatabaseCommand Create(string commandText, CommandType commandType);

        IDatabaseCommand Create(string commandText, CommandType commandType, IDictionary<string, object> parameters);
    }
}
