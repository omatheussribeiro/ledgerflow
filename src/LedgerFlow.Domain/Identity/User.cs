using LedgerFlow.Domain.Common;

namespace LedgerFlow.Domain.Identity;

public sealed record User(Guid Id, string Name, string Email, string PasswordHash, string Role, DateTimeOffset CreatedAt)
{
    public static User Create(string name, string email, string passwordHash, string role = "User")
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) throw new DomainException("A valid email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException("Password hash is required.");

        return new User(Guid.NewGuid(), name.Trim(), email.Trim().ToLowerInvariant(), passwordHash, role, DateTimeOffset.UtcNow);
    }
}
