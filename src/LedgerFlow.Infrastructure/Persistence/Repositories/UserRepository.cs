using Dapper;
using LedgerFlow.Application.Interfaces.Repositories;
using LedgerFlow.Domain.Identity;
using LedgerFlow.Infrastructure.Persistence.Context;
using LedgerFlow.Infrastructure.Persistence.Context.Configurations;

namespace LedgerFlow.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(LedgerFlowDbContext context) : IUserRepository
{
    public async Task<User?> FindByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT {UserTableConfiguration.ReadProjection}
            FROM {UserTableConfiguration.TableName}
            WHERE NormalizedEmail = @NormalizedEmail;
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(
                sql,
                new { NormalizedEmail = normalizedEmail.ToUpperInvariant() },
                cancellationToken: cancellationToken));
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = $"SELECT {UserTableConfiguration.ReadProjection} FROM {UserTableConfiguration.TableName} WHERE Id = @Id;";
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        var sql = $"""
            INSERT INTO {UserTableConfiguration.TableName} (Id, Name, Email, NormalizedEmail, PasswordHash, Role, CreatedAt)
            VALUES (@Id, @Name, @Email, @NormalizedEmail, @PasswordHash, @Role, @CreatedAt);
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            user.Id,
            user.Name,
            user.Email,
            NormalizedEmail = user.Email.ToUpperInvariant(),
            user.PasswordHash,
            user.Role,
            user.CreatedAt
        }, cancellationToken: cancellationToken));
    }

    public async Task StoreRefreshTokenAsync(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            INSERT INTO {RefreshTokenTableConfiguration.TableName} (Id, UserId, TokenHash, ExpiresAt, CreatedAt)
            VALUES (@Id, @UserId, @TokenHash, @ExpiresAt, SYSUTCDATETIME());
            """;
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Id = Guid.NewGuid(), UserId = userId, TokenHash = tokenHash, ExpiresAt = expiresAt },
                cancellationToken: cancellationToken));
    }

    public async Task<Guid?> RotateRefreshTokenAsync(
        string currentHash,
        string nextHash,
        DateTimeOffset nextExpiresAt,
        CancellationToken cancellationToken)
    {
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var selectSql = $"""
            SELECT UserId FROM {RefreshTokenTableConfiguration.TableName} WITH (UPDLOCK, ROWLOCK)
            WHERE TokenHash = @TokenHash AND RevokedAt IS NULL AND ExpiresAt > SYSUTCDATETIME();
            """;
        var userId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition(
                selectSql,
                new { TokenHash = currentHash },
                transaction,
                cancellationToken: cancellationToken));
        if (userId is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var rotateSql = $"""
            UPDATE {RefreshTokenTableConfiguration.TableName} SET RevokedAt = SYSUTCDATETIME(), ReplacedByTokenHash = @NextHash WHERE TokenHash = @CurrentHash;
            INSERT INTO {RefreshTokenTableConfiguration.TableName} (Id, UserId, TokenHash, ExpiresAt, CreatedAt)
            VALUES (@Id, @UserId, @NextHash, @ExpiresAt, SYSUTCDATETIME());
            """;
        await connection.ExecuteAsync(new CommandDefinition(rotateSql, new
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            CurrentHash = currentHash,
            NextHash = nextHash,
            ExpiresAt = nextExpiresAt
        }, transaction, cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return userId;
    }

    public async Task RevokeRefreshTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        var sql = $"UPDATE {RefreshTokenTableConfiguration.TableName} SET RevokedAt = COALESCE(RevokedAt, SYSUTCDATETIME()) WHERE TokenHash = @TokenHash;";
        await using var connection = await context.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { TokenHash = tokenHash }, cancellationToken: cancellationToken));
    }
}
