namespace ClickerHeroesTrackerWebsite.Database
{
    using System.Data.SqlClient;

    public interface IDatabaseCommand
    {
        void AddParameter(string parameterName, object value);

        void ExecuteNonQuery();

        // BUGBUG 59 - Remove the raw SqlDataReader usage
        SqlDataReader ExecuteReader();
    }
}
