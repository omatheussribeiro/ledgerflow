using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace LedgerFlow.Infrastructure.Persistence.Context;

public sealed class LedgerFlowDbContext
{
    public LedgerFlowDbContext(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A database connection string is required.", nameof(connectionString));
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
