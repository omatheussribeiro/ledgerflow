using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Domain.Financial;

public sealed class Category
{
    private Category() { }

    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public string Name { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public string Color { get; private set; } = "#64748b";
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Category Create(Guid userId, string name, TransactionType type, string? color)
    {
        if (userId == Guid.Empty) throw new DomainException("A valid owner is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Category name is required.");
        if (name.Trim().Length > 80) throw new DomainException("Category name cannot exceed 80 characters.");

        var normalizedColor = string.IsNullOrWhiteSpace(color) ? "#64748b" : color.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedColor, "^#[0-9a-fA-F]{6}$"))
            throw new DomainException("Color must be a six-digit hexadecimal value.");

        return new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            Type = type,
            Color = normalizedColor,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string name, TransactionType type, string? color)
    {
        if (DeletedAt is not null) throw new DomainException("Deleted categories cannot be updated.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Category name is required.");
        if (name.Trim().Length > 80) throw new DomainException("Category name cannot exceed 80 characters.");
        if (!Enum.IsDefined(type)) throw new DomainException("Transaction type is invalid.");

        var normalizedColor = string.IsNullOrWhiteSpace(color) ? "#64748b" : color.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedColor, "^#[0-9a-fA-F]{6}$"))
            throw new DomainException("Color must be a six-digit hexadecimal value.");

        Name = name.Trim();
        Type = type;
        Color = normalizedColor;
    }

    public void Delete(DateTimeOffset deletedAt)
    {
        if (DeletedAt is null) DeletedAt = deletedAt;
    }
}
