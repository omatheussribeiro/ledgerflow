using System.Data.Common;
using LedgerFlow.Domain.Financial;
using LedgerFlow.Domain.Identity;
using LedgerFlow.Infrastructure.Persistence.Context.Configurations;
using LedgerFlow.Infrastructure.Persistence.Context.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Context;

public sealed class LedgerFlowDbContext(DbContextOptions<LedgerFlowDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<FinancialTransaction> Transactions => Set<FinancialTransaction>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public string ConnectionString => Database.GetConnectionString()
        ?? throw new InvalidOperationException("The database connection string is not configured.");

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .ConfigureUserTable()
            .ConfigureAccountTable()
            .ConfigureCategoryTable()
            .ConfigureTransactionTable()
            .ConfigureRefreshTokenTable();
    }
}
