using Dapper;
using LedgerFlow.Application.Interfaces.Services;
using LedgerFlow.Infrastructure.Persistence.Context.Configurations;

namespace LedgerFlow.Infrastructure.Persistence.Context;

public sealed class DevelopmentDataSeeder(LedgerFlowDbContext context, IPasswordService passwords)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existsSql = $"SELECT COUNT(1) FROM {UserTableConfiguration.TableName} WHERE NormalizedEmail = 'DEMO@LEDGERFLOW.DEV';";
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        if (await connection.QuerySingleAsync<int>(
                new CommandDefinition(existsSql, cancellationToken: cancellationToken)) > 0)
            return;

        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var checkingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var salaryId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var housingId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var sql = $"""
            INSERT INTO {UserTableConfiguration.TableName} (Id, Name, Email, NormalizedEmail, PasswordHash, Role, CreatedAt)
            VALUES (@UserId, 'Demo User', 'demo@ledgerflow.dev', 'DEMO@LEDGERFLOW.DEV', @PasswordHash, 'User', SYSUTCDATETIME());
            INSERT INTO {AccountTableConfiguration.TableName} (Id, UserId, Name, Type, InitialBalance, IsActive, CreatedAt)
            VALUES (@CheckingId, @UserId, 'Main account', 1, 2500.00, 1, SYSUTCDATETIME());
            INSERT INTO {CategoryTableConfiguration.TableName} (Id, UserId, Name, Type, Color, CreatedAt) VALUES
                (@SalaryId, @UserId, 'Salary', 1, '#22c55e', SYSUTCDATETIME()),
                (@HousingId, @UserId, 'Housing', 2, '#f97316', SYSUTCDATETIME());
            INSERT INTO {TransactionTableConfiguration.TableName} (Id, UserId, AccountId, CategoryId, Type, Description, Amount, OccurredOn, Status, CreatedAt) VALUES
                (NEWID(), @UserId, @CheckingId, @SalaryId, 1, 'Monthly salary', 6500.00, CAST(SYSUTCDATETIME() AS date), 2, SYSUTCDATETIME()),
                (NEWID(), @UserId, @CheckingId, @HousingId, 2, 'Rent', 1800.00, CAST(SYSUTCDATETIME() AS date), 2, SYSUTCDATETIME());
            """;
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            UserId = userId,
            CheckingId = checkingId,
            SalaryId = salaryId,
            HousingId = housingId,
            PasswordHash = passwords.Hash("DemoPass123!")
        }, cancellationToken: cancellationToken));
    }
}
