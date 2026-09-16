using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Infrastructure.Persistence.Repositories.Models;

internal sealed class TransactionReadModel
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public byte Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime OccurredOn { get; set; }
    public byte Status { get; set; }
    public string? Notes { get; set; }

    public TransactionResponseDto ToDto() => new(
        Id,
        AccountId,
        AccountName,
        CategoryId,
        CategoryName,
        CategoryColor,
        (TransactionType)Type,
        Description,
        Amount,
        DateOnly.FromDateTime(OccurredOn),
        (TransactionStatus)Status,
        Notes);
}
