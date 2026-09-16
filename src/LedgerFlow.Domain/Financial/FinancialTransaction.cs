using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Domain.Financial;

public sealed class FinancialTransaction
{
    private FinancialTransaction() { }

    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public Guid AccountId { get; private init; }
    public Guid CategoryId { get; private init; }
    public TransactionType Type { get; private init; }
    public string Description { get; private init; } = string.Empty;
    public decimal Amount { get; private init; }
    public DateOnly OccurredOn { get; private init; }
    public TransactionStatus Status { get; private init; }
    public string? Notes { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }

    public static FinancialTransaction Create(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        TransactionType type,
        string description,
        decimal amount,
        DateOnly occurredOn,
        TransactionStatus status = TransactionStatus.Cleared,
        string? notes = null)
    {
        if (userId == Guid.Empty || accountId == Guid.Empty || categoryId == Guid.Empty)
            throw new DomainException("Owner, account and category are required.");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required.");
        if (description.Trim().Length > 160) throw new DomainException("Description cannot exceed 160 characters.");
        if (amount <= 0) throw new DomainException("Amount must be greater than zero.");
        if (notes?.Length > 500) throw new DomainException("Notes cannot exceed 500 characters.");

        return new FinancialTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Type = type,
            Description = description.Trim(),
            Amount = decimal.Round(amount, 2),
            OccurredOn = occurredOn,
            Status = status,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
