namespace ClickerHeroesTrackerWebsite.Database
{
    using System;
    using System.Data.SqlClient;

    public interface IDatabaseCommand : IDisposable
    {
        void ExecuteNonQuery();

        // BUGBUG 59 - Remove the raw SqlDataReader usage
        SqlDataReader ExecuteReader();
    }
}
