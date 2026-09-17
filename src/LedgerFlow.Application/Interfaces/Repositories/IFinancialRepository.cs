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
    Task<AccountResponseDto?> UpdateAccountAsync(
        Guid userId,
        Guid accountId,
        UpdateAccountRequestDto request,
        CancellationToken cancellationToken);
    Task<bool> DeleteAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryResponseDto>> GetCategoriesAsync(Guid userId, CancellationToken cancellationToken);
    Task AddCategoryAsync(Category category, CancellationToken cancellationToken);
    Task<CategoryResponseDto?> UpdateCategoryAsync(
        Guid userId,
        Guid categoryId,
        UpdateCategoryRequestDto request,
        CancellationToken cancellationToken);
    Task<bool> DeleteCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken);
    Task<bool> OwnsAccountAndCategoryAsync(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        TransactionType type,
        CancellationToken cancellationToken);
    Task AddTransactionAsync(FinancialTransaction transaction, CancellationToken cancellationToken);
    Task<TransactionResponseDto?> UpdateTransactionAsync(
        Guid userId,
        Guid transactionId,
        UpdateTransactionRequestDto request,
        CancellationToken cancellationToken);
    Task<bool> DeleteTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken);
    Task<PagedResult<TransactionResponseDto>> GetTransactionsAsync(
        Guid userId,
        TransactionFilterDto filter,
        CancellationToken cancellationToken);
    Task<DashboardResponseDto> GetDashboardAsync(
        Guid userId,
        DateOnly month,
        CancellationToken cancellationToken);
}
