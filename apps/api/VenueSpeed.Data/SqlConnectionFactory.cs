using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace VenueSpeed.Data;

public class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration config)
        => _connectionString = config["AZURE_SQL_CONNECTION_STRING"]
            ?? throw new InvalidOperationException("AZURE_SQL_CONNECTION_STRING environment variable is not set.");

    public SqlConnection Create() => new SqlConnection(_connectionString);
}
