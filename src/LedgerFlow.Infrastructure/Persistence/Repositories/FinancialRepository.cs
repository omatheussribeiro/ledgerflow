using System.Text;
using Dapper;
using LedgerFlow.Application.Common;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Application.Interfaces.Repositories;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Financial;
using LedgerFlow.Infrastructure.Persistence.Context;
using LedgerFlow.Infrastructure.Persistence.Context.Configurations;
using LedgerFlow.Infrastructure.Persistence.Repositories.Models;

namespace LedgerFlow.Infrastructure.Persistence.Repositories;

public sealed class FinancialRepository(LedgerFlowDbContext context) : IFinancialRepository
{
    public async Task<IReadOnlyList<AccountResponseDto>> GetAccountsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT {AccountTableConfiguration.ReadProjection},
                   a.InitialBalance + COALESCE(SUM(CASE WHEN t.Status = 2 AND t.Type = 1 THEN t.Amount WHEN t.Status = 2 AND t.Type = 2 THEN -t.Amount ELSE 0 END), 0) AS Balance,
                   a.IsActive
            FROM {AccountTableConfiguration.TableName} a
            LEFT JOIN {TransactionTableConfiguration.TableName} t ON t.AccountId = a.Id
            WHERE a.UserId = @UserId
            GROUP BY a.Id, a.Name, a.Type, a.InitialBalance, a.IsActive
            ORDER BY a.Name;
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<AccountReadModel>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
        return rows.Select(row => row.ToDto()).ToArray();
    }

    public async Task<AccountResponseDto?> GetAccountAsync(
        Guid userId,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT {AccountTableConfiguration.ReadProjection},
                   a.InitialBalance + COALESCE(SUM(CASE WHEN t.Status = 2 AND t.Type = 1 THEN t.Amount WHEN t.Status = 2 AND t.Type = 2 THEN -t.Amount ELSE 0 END), 0) AS Balance,
                   a.IsActive
            FROM {AccountTableConfiguration.TableName} a
            LEFT JOIN {TransactionTableConfiguration.TableName} t ON t.AccountId = a.Id
            WHERE a.UserId = @UserId AND a.Id = @AccountId
            GROUP BY a.Id, a.Name, a.Type, a.InitialBalance, a.IsActive;
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<AccountReadModel>(
            new CommandDefinition(
                sql,
                new { UserId = userId, AccountId = accountId },
                cancellationToken: cancellationToken));
        return row?.ToDto();
    }

    public async Task AddAccountAsync(Account account, CancellationToken cancellationToken)
    {
        var sql = $"""
            INSERT INTO {AccountTableConfiguration.TableName} (Id, UserId, Name, Type, InitialBalance, IsActive, CreatedAt)
            VALUES (@Id, @UserId, @Name, @Type, @InitialBalance, @IsActive, @CreatedAt);
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, account, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<CategoryResponseDto>> GetCategoriesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var sql = $"SELECT {CategoryTableConfiguration.ReadProjection} FROM {CategoryTableConfiguration.TableName} c WHERE c.UserId = @UserId ORDER BY c.Type, c.Name;";
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<CategoryReadModel>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
        return rows.Select(row => row.ToDto()).ToArray();
    }

    public async Task AddCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        var sql = $"""
            INSERT INTO {CategoryTableConfiguration.TableName} (Id, UserId, Name, Type, Color, CreatedAt)
            VALUES (@Id, @UserId, @Name, @Type, @Color, @CreatedAt);
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, category, cancellationToken: cancellationToken));
    }

    public async Task<bool> OwnsAccountAndCategoryAsync(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        TransactionType type,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT CASE WHEN EXISTS (SELECT 1 FROM {AccountTableConfiguration.TableName} WHERE Id = @AccountId AND UserId = @UserId AND IsActive = 1)
                         AND EXISTS (SELECT 1 FROM {CategoryTableConfiguration.TableName} WHERE Id = @CategoryId AND UserId = @UserId AND Type = @Type)
                        THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END;
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                sql,
                new { UserId = userId, AccountId = accountId, CategoryId = categoryId, Type = type },
                cancellationToken: cancellationToken));
    }

    public async Task AddTransactionAsync(
        FinancialTransaction transaction,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            INSERT INTO {TransactionTableConfiguration.TableName} (Id, UserId, AccountId, CategoryId, Type, Description, Amount, OccurredOn, Status, Notes, CreatedAt)
            VALUES (@Id, @UserId, @AccountId, @CategoryId, @Type, @Description, @Amount, @OccurredOn, @Status, @Notes, @CreatedAt);
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
    }

    public async Task<PagedResult<TransactionResponseDto>> GetTransactionsAsync(
        Guid userId,
        TransactionFilterDto filter,
        CancellationToken cancellationToken)
    {
        var where = new StringBuilder(" WHERE t.UserId = @UserId");
        var parameters = new DynamicParameters(new { UserId = userId });
        if (filter.From is not null)
        {
            where.Append(" AND t.OccurredOn >= @From");
            parameters.Add("From", filter.From);
        }

        if (filter.To is not null)
        {
            where.Append(" AND t.OccurredOn <= @To");
            parameters.Add("To", filter.To);
        }

        if (filter.AccountId is not null)
        {
            where.Append(" AND t.AccountId = @AccountId");
            parameters.Add("AccountId", filter.AccountId);
        }

        if (filter.CategoryId is not null)
        {
            where.Append(" AND t.CategoryId = @CategoryId");
            parameters.Add("CategoryId", filter.CategoryId);
        }

        if (filter.Type is not null)
        {
            where.Append(" AND t.Type = @Type");
            parameters.Add("Type", filter.Type);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            where.Append(" AND t.Description LIKE @Search ESCAPE '\\'");
            parameters.Add("Search", $"%{EscapeLike(filter.Search.Trim())}%");
        }

        parameters.Add("Offset", (filter.SafePage - 1) * filter.SafePageSize);
        parameters.Add("PageSize", filter.SafePageSize);

        var sql = $"""
            SELECT {TransactionTableConfiguration.ReadProjection}
            FROM {TransactionTableConfiguration.TableName} t
            INNER JOIN {AccountTableConfiguration.TableName} a ON a.Id = t.AccountId
            INNER JOIN {CategoryTableConfiguration.TableName} c ON c.Id = t.CategoryId
            {where}
            ORDER BY t.OccurredOn DESC, t.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            SELECT COUNT_BIG(1) FROM {TransactionTableConfiguration.TableName} t {where};
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        using var grid = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        var items = (await grid.ReadAsync<TransactionReadModel>())
            .Select(row => row.ToDto())
            .ToArray();
        var total = checked((int)await grid.ReadSingleAsync<long>());
        return new PagedResult<TransactionResponseDto>(
            items,
            filter.SafePage,
            filter.SafePageSize,
            total);
    }

    public async Task<DashboardResponseDto> GetDashboardAsync(
        Guid userId,
        DateOnly month,
        CancellationToken cancellationToken)
    {
        var start = new DateOnly(month.Year, month.Month, 1);
        var end = start.AddMonths(1);
        var sql = $"""
            SELECT COALESCE(SUM(a.InitialBalance), 0) + COALESCE((SELECT SUM(CASE WHEN Type = 1 THEN Amount ELSE -Amount END) FROM {TransactionTableConfiguration.TableName} WHERE UserId = @UserId AND Status = 2), 0)
            FROM {AccountTableConfiguration.TableName} a WHERE a.UserId = @UserId AND a.IsActive = 1;

            SELECT COALESCE(SUM(CASE WHEN Type = 1 THEN Amount ELSE 0 END), 0) AS MonthlyIncome,
                   COALESCE(SUM(CASE WHEN Type = 2 THEN Amount ELSE 0 END), 0) AS MonthlyExpense
            FROM {TransactionTableConfiguration.TableName} WHERE UserId = @UserId AND Status = 2 AND OccurredOn >= @Start AND OccurredOn < @End;

            WITH Months AS (
                SELECT 0 AS N, DATEADD(month, DATEDIFF(month, 0, @Start) - 5, 0) AS MonthStart
                UNION ALL SELECT N + 1, DATEADD(month, 1, MonthStart) FROM Months WHERE N < 5
            )
            SELECT YEAR(m.MonthStart) AS [Year], MONTH(m.MonthStart) AS [Month],
                   COALESCE(SUM(CASE WHEN t.Type = 1 THEN t.Amount ELSE 0 END), 0) AS Income,
                   COALESCE(SUM(CASE WHEN t.Type = 2 THEN t.Amount ELSE 0 END), 0) AS Expense
            FROM Months m LEFT JOIN {TransactionTableConfiguration.TableName} t ON t.UserId = @UserId AND t.Status = 2
                AND t.OccurredOn >= m.MonthStart AND t.OccurredOn < DATEADD(month, 1, m.MonthStart)
            GROUP BY m.MonthStart ORDER BY m.MonthStart OPTION (MAXRECURSION 6);

            SELECT c.Id AS CategoryId, c.Name, c.Color, SUM(t.Amount) AS Total
            FROM {TransactionTableConfiguration.TableName} t INNER JOIN {CategoryTableConfiguration.TableName} c ON c.Id = t.CategoryId
            WHERE t.UserId = @UserId AND t.Type = 2 AND t.Status = 2 AND t.OccurredOn >= @Start AND t.OccurredOn < @End
            GROUP BY c.Id, c.Name, c.Color ORDER BY Total DESC;

            SELECT TOP (8) {TransactionTableConfiguration.ReadProjection}
            FROM {TransactionTableConfiguration.TableName} t
            INNER JOIN {AccountTableConfiguration.TableName} a ON a.Id = t.AccountId
            INNER JOIN {CategoryTableConfiguration.TableName} c ON c.Id = t.CategoryId
            WHERE t.UserId = @UserId ORDER BY t.OccurredOn DESC, t.CreatedAt DESC;
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        using var grid = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new { UserId = userId, Start = start, End = end },
                cancellationToken: cancellationToken));
        var totalBalance = await grid.ReadSingleAsync<decimal>();
        var summary = await grid.ReadSingleAsync<MonthlySummary>();
        var evolution = (await grid.ReadAsync<MonthlyPointDto>()).AsList();
        var categories = (await grid.ReadAsync<CategoryTotalDto>()).AsList();
        var latest = (await grid.ReadAsync<TransactionReadModel>())
            .Select(row => row.ToDto())
            .ToArray();
        return new DashboardResponseDto(
            totalBalance,
            summary.MonthlyIncome,
            summary.MonthlyExpense,
            summary.MonthlyIncome - summary.MonthlyExpense,
            evolution,
            categories,
            latest);
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");

    private sealed record MonthlySummary(decimal MonthlyIncome, decimal MonthlyExpense);
}
