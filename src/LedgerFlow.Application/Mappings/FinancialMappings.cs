using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Domain.Financial;

namespace LedgerFlow.Application.Mappings;

public static class FinancialMappings
{
    public static AccountResponseDto ToDto(this Account account, decimal? balance = null) =>
        new(
            account.Id,
            account.Name,
            account.Type,
            account.InitialBalance,
            balance ?? account.InitialBalance,
            account.IsActive);

    public static CategoryResponseDto ToDto(this Category category) =>
        new(category.Id, category.Name, category.Type, category.Color);

    public static TransactionResponseDto ToDto(
        this FinancialTransaction transaction,
        AccountResponseDto account,
        CategoryResponseDto category) =>
        new(
            transaction.Id,
            transaction.AccountId,
            account.Name,
            transaction.CategoryId,
            category.Name,
            category.Color,
            transaction.Type,
            transaction.Description,
            transaction.Amount,
            transaction.OccurredOn,
            transaction.Status,
            transaction.Notes);
}
