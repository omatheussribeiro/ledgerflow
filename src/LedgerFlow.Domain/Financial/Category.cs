using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Domain.Financial;

public sealed class Category
{
    private Category() { }

    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public string Name { get; private init; } = string.Empty;
    public TransactionType Type { get; private init; }
    public string Color { get; private init; } = "#64748b";
    public DateTimeOffset CreatedAt { get; private init; }

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
}
