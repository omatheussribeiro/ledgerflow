using LedgerFlow.Application.Common;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Application.Interfaces.Repositories;
using LedgerFlow.Application.Interfaces.Services;
using LedgerFlow.Application.Mappings;
using LedgerFlow.Domain.Financial;

namespace LedgerFlow.Application.Services;

public sealed class FinancialService(
    IFinancialRepository repository,
    IRequestValidator<CreateAccountRequestDto> accountValidator,
    IRequestValidator<UpdateAccountRequestDto> updateAccountValidator,
    IRequestValidator<CreateCategoryRequestDto> categoryValidator,
    IRequestValidator<UpdateCategoryRequestDto> updateCategoryValidator,
    IRequestValidator<CreateTransactionRequestDto> transactionValidator,
    IRequestValidator<UpdateTransactionRequestDto> updateTransactionValidator,
    IRequestValidator<TransactionFilterDto> filterValidator) : IFinancialService
{
    public Task<IReadOnlyList<AccountResponseDto>> GetAccountsAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        repository.GetAccountsAsync(userId, cancellationToken);

    public async Task<AccountResponseDto> CreateAccountAsync(
        Guid userId,
        CreateAccountRequestDto request,
        CancellationToken cancellationToken)
    {
        accountValidator.ValidateAndThrow(request);
        var account = Account.Create(userId, request.Name, request.Type, request.InitialBalance);
        await repository.AddAccountAsync(account, cancellationToken);
        return account.ToDto();
    }

    public async Task<AccountResponseDto> UpdateAccountAsync(
        Guid userId,
        Guid accountId,
        UpdateAccountRequestDto request,
        CancellationToken cancellationToken)
    {
        updateAccountValidator.ValidateAndThrow(request);
        return await repository.UpdateAccountAsync(userId, accountId, request, cancellationToken)
            ?? throw new NotFoundException("Account was not found.");
    }

    public async Task DeleteAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken)
    {
        if (!await repository.DeleteAccountAsync(userId, accountId, cancellationToken))
            throw new NotFoundException("Account was not found.");
    }

    public Task<IReadOnlyList<CategoryResponseDto>> GetCategoriesAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        repository.GetCategoriesAsync(userId, cancellationToken);

    public async Task<CategoryResponseDto> CreateCategoryAsync(
        Guid userId,
        CreateCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        categoryValidator.ValidateAndThrow(request);
        var category = Category.Create(userId, request.Name, request.Type, request.Color);
        await repository.AddCategoryAsync(category, cancellationToken);
        return category.ToDto();
    }

    public async Task<CategoryResponseDto> UpdateCategoryAsync(
        Guid userId,
        Guid categoryId,
        UpdateCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        updateCategoryValidator.ValidateAndThrow(request);
        return await repository.UpdateCategoryAsync(userId, categoryId, request, cancellationToken)
            ?? throw new NotFoundException("Category was not found.");
    }

    public async Task DeleteCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken)
    {
        if (!await repository.DeleteCategoryAsync(userId, categoryId, cancellationToken))
            throw new NotFoundException("Category was not found.");
    }

    public Task<PagedResult<TransactionResponseDto>> GetTransactionsAsync(
        Guid userId,
        TransactionFilterDto filter,
        CancellationToken cancellationToken)
    {
        filterValidator.ValidateAndThrow(filter);
        return repository.GetTransactionsAsync(userId, filter, cancellationToken);
    }

    public async Task<TransactionResponseDto> CreateTransactionAsync(
        Guid userId,
        CreateTransactionRequestDto request,
        CancellationToken cancellationToken)
    {
        transactionValidator.ValidateAndThrow(request);
        var ownsReferences = await repository.OwnsAccountAndCategoryAsync(
            userId,
            request.AccountId,
            request.CategoryId,
            request.Type,
            cancellationToken);
        if (!ownsReferences)
            throw new NotFoundException("Account or category was not found, or the category type does not match.");

        var transaction = FinancialTransaction.Create(
            userId,
            request.AccountId,
            request.CategoryId,
            request.Type,
            request.Description,
            request.Amount,
            request.OccurredOn,
            request.Status,
            request.Notes);
        await repository.AddTransactionAsync(transaction, cancellationToken);

        var account = await repository.GetAccountAsync(userId, request.AccountId, cancellationToken)
            ?? throw new NotFoundException("Account was not found after creating the transaction.");
        var category = (await repository.GetCategoriesAsync(userId, cancellationToken))
            .Single(item => item.Id == request.CategoryId);
        return transaction.ToDto(account, category);
    }

    public async Task<TransactionResponseDto> UpdateTransactionAsync(
        Guid userId,
        Guid transactionId,
        UpdateTransactionRequestDto request,
        CancellationToken cancellationToken)
    {
        updateTransactionValidator.ValidateAndThrow(request);
        var ownsReferences = await repository.OwnsAccountAndCategoryAsync(
            userId,
            request.AccountId,
            request.CategoryId,
            request.Type,
            cancellationToken);
        if (!ownsReferences)
            throw new NotFoundException("Account or category was not found, or the category type does not match.");

        return await repository.UpdateTransactionAsync(userId, transactionId, request, cancellationToken)
            ?? throw new NotFoundException("Transaction was not found.");
    }

    public async Task DeleteTransactionAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        if (!await repository.DeleteTransactionAsync(userId, transactionId, cancellationToken))
            throw new NotFoundException("Transaction was not found.");
    }

    public Task<DashboardResponseDto> GetDashboardAsync(
        Guid userId,
        DateOnly month,
        CancellationToken cancellationToken) =>
        repository.GetDashboardAsync(userId, month, cancellationToken);
}
