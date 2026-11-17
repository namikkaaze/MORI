using Microsoft.Data.SqlClient;

namespace mori;

internal static class Db
{
    private const string DatabaseName = "moridb_v2";

    
    private const string ServerName = @"(localdb)\MSSQLLocalDB";

    public static SqlConnection CreateConnection()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = ServerName,
            InitialCatalog = DatabaseName,
            IntegratedSecurity = true,
            ["Encrypt"] = true,
            ["TrustServerCertificate"] = true,
            ConnectTimeout = 5,
            ApplicationName = "MORI"
        };

        return new SqlConnection(builder.ConnectionString);
    }
}
