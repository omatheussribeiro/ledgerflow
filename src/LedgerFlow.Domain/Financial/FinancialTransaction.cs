using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Domain.Financial;

public sealed class FinancialTransaction
{
    private FinancialTransaction() { }

    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public Guid AccountId { get; private set; }
    public Guid CategoryId { get; private set; }
    public TransactionType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateOnly OccurredOn { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? DeletedAt { get; private set; }

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

    public void Update(
        Guid accountId,
        Guid categoryId,
        TransactionType type,
        string description,
        decimal amount,
        DateOnly occurredOn,
        TransactionStatus status,
        string? notes)
    {
        if (DeletedAt is not null) throw new DomainException("Deleted transactions cannot be updated.");
        if (accountId == Guid.Empty || categoryId == Guid.Empty)
            throw new DomainException("Account and category are required.");
        if (!Enum.IsDefined(type)) throw new DomainException("Transaction type is invalid.");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required.");
        if (description.Trim().Length > 160) throw new DomainException("Description cannot exceed 160 characters.");
        if (amount <= 0) throw new DomainException("Amount must be greater than zero.");
        if (occurredOn == default) throw new DomainException("A valid occurrence date is required.");
        if (!Enum.IsDefined(status)) throw new DomainException("Transaction status is invalid.");
        if (notes?.Length > 500) throw new DomainException("Notes cannot exceed 500 characters.");

        AccountId = accountId;
        CategoryId = categoryId;
        Type = type;
        Description = description.Trim();
        Amount = decimal.Round(amount, 2);
        OccurredOn = occurredOn;
        Status = status;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void Delete(DateTimeOffset deletedAt)
    {
        if (DeletedAt is null) DeletedAt = deletedAt;
    }
}
