using System.ComponentModel.DataAnnotations;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Application.Dtos.Financial;

/// <summary>Data required to record a financial movement.</summary>
public sealed record CreateTransactionRequestDto(
    Guid AccountId,
    Guid CategoryId,
    [EnumDataType(typeof(TransactionType))] TransactionType Type,
    [Required, StringLength(160)] string Description,
    [Range(0.01, 999999999.99)] decimal Amount,
    DateOnly OccurredOn,
    [EnumDataType(typeof(TransactionStatus))] TransactionStatus Status = TransactionStatus.Cleared,
    [StringLength(500)] string? Notes = null);

/// <summary>A transaction enriched with account and category display data.</summary>
public sealed record TransactionResponseDto(
    Guid Id,
    Guid AccountId,
    string AccountName,
    Guid CategoryId,
    string CategoryName,
    string CategoryColor,
    TransactionType Type,
    string Description,
    decimal Amount,
    DateOnly OccurredOn,
    TransactionStatus Status,
    string? Notes);

/// <summary>Optional ledger filters and pagination controls.</summary>
public sealed record TransactionFilterDto(
    DateOnly? From,
    DateOnly? To,
    Guid? AccountId,
    Guid? CategoryId,
    TransactionType? Type,
    string? Search,
    int Page = 1,
    int PageSize = 20)
{
    public int SafePage => Math.Max(1, Page);
    public int SafePageSize => Math.Clamp(PageSize, 1, 100);
}
