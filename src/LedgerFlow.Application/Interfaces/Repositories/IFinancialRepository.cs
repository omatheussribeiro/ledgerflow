using LedgerFlow.Application.Common;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Financial;

namespace LedgerFlow.Application.Interfaces.Repositories;

public interface IFinancialRepository
{
    Task<IReadOnlyList<AccountResponseDto>> GetAccountsAsync(Guid userId, CancellationToken cancellationToken);
    Task<AccountResponseDto?> GetAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken);
    Task AddAccountAsync(Account account, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryResponseDto>> GetCategoriesAsync(Guid userId, CancellationToken cancellationToken);
    Task AddCategoryAsync(Category category, CancellationToken cancellationToken);
    Task<bool> OwnsAccountAndCategoryAsync(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        TransactionType type,
        CancellationToken cancellationToken);
    Task AddTransactionAsync(FinancialTransaction transaction, CancellationToken cancellationToken);
    Task<PagedResult<TransactionResponseDto>> GetTransactionsAsync(
        Guid userId,
        TransactionFilterDto filter,
        CancellationToken cancellationToken);
    Task<DashboardResponseDto> GetDashboardAsync(
        Guid userId,
        DateOnly month,
        CancellationToken cancellationToken);
}
