using System.Reflection;
using DbUp;
using LedgerFlow.Infrastructure.Persistence.Context.Configurations;

namespace LedgerFlow.Infrastructure.Persistence.Context;

public sealed class DatabaseInitializer(LedgerFlowDbContext context)
{
    public void Migrate()
    {
        EnsureDatabase.For.SqlDatabase(context.ConnectionString);
        var result = DeployChanges.To
            .SqlDatabase(context.ConnectionString)
            .JournalToSqlTable(DatabaseConfiguration.Schema, DatabaseConfiguration.MigrationJournalTable)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToConsole()
            .Build()
            .PerformUpgrade();
        if (!result.Successful)
            throw new InvalidOperationException(
                $"Database migration failed: {result.Error.Message}",
                result.Error);
    }
}
