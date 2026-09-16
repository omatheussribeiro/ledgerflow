namespace LedgerFlow.Application.Dtos.Financial;

/// <summary>Income and expense totals for one calendar month.</summary>
public sealed record MonthlyPointDto(int Year, int Month, decimal Income, decimal Expense);

/// <summary>Expense total grouped by category.</summary>
public sealed record CategoryTotalDto(Guid CategoryId, string Name, string Color, decimal Total);

/// <summary>Consolidated financial dashboard for a selected month.</summary>
public sealed record DashboardResponseDto(
    decimal TotalBalance,
    decimal MonthlyIncome,
    decimal MonthlyExpense,
    decimal MonthlyResult,
    IReadOnlyList<MonthlyPointDto> Evolution,
    IReadOnlyList<CategoryTotalDto> ExpensesByCategory,
    IReadOnlyList<TransactionResponseDto> LatestTransactions);
