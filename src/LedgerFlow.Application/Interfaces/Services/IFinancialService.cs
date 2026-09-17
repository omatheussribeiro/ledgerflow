using LedgerFlow.Application.Common;
using LedgerFlow.Application.Dtos.Financial;

namespace LedgerFlow.Application.Interfaces.Services;

public interface IFinancialService
{
    Task<IReadOnlyList<AccountResponseDto>> GetAccountsAsync(Guid userId, CancellationToken cancellationToken);
    Task<AccountResponseDto> CreateAccountAsync(
        Guid userId,
        CreateAccountRequestDto request,
        CancellationToken cancellationToken);
    Task<AccountResponseDto> UpdateAccountAsync(
        Guid userId,
        Guid accountId,
        UpdateAccountRequestDto request,
        CancellationToken cancellationToken);
    Task DeleteAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryResponseDto>> GetCategoriesAsync(Guid userId, CancellationToken cancellationToken);
    Task<CategoryResponseDto> CreateCategoryAsync(
        Guid userId,
        CreateCategoryRequestDto request,
        CancellationToken cancellationToken);
    Task<CategoryResponseDto> UpdateCategoryAsync(
        Guid userId,
        Guid categoryId,
        UpdateCategoryRequestDto request,
        CancellationToken cancellationToken);
    Task DeleteCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken);
    Task<PagedResult<TransactionResponseDto>> GetTransactionsAsync(
        Guid userId,
        TransactionFilterDto filter,
        CancellationToken cancellationToken);
    Task<TransactionResponseDto> CreateTransactionAsync(
        Guid userId,
        CreateTransactionRequestDto request,
        CancellationToken cancellationToken);
    Task<TransactionResponseDto> UpdateTransactionAsync(
        Guid userId,
        Guid transactionId,
        UpdateTransactionRequestDto request,
        CancellationToken cancellationToken);
    Task DeleteTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken);
    Task<DashboardResponseDto> GetDashboardAsync(
        Guid userId,
        DateOnly month,
        CancellationToken cancellationToken);
}
