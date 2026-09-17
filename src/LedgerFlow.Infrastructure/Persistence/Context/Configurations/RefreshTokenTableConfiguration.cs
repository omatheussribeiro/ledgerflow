using LedgerFlow.Domain.Identity;
using LedgerFlow.Infrastructure.Persistence.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Context.Configurations;

public static class RefreshTokenTableConfiguration
{
    public const string TableName = "[dbo].[RefreshTokens]";

    public static ModelBuilder ConfigureRefreshTokenTable(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens", DatabaseConfiguration.Schema);

            entity.HasKey(token => token.Id).HasName("PK_RefreshTokens");
            entity.Property(token => token.TokenHash)
                .HasColumnType("char(64)")
                .IsUnicode(false)
                .IsFixedLength()
                .IsRequired();
            entity.Property(token => token.ExpiresAt).HasColumnType("datetimeoffset(0)");
            entity.Property(token => token.CreatedAt).HasColumnType("datetimeoffset(0)");
            entity.Property(token => token.RevokedAt).HasColumnType("datetimeoffset(0)");
            entity.Property(token => token.ReplacedByTokenHash)
                .HasColumnType("char(64)")
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasIndex(token => token.TokenHash)
                .IsUnique()
                .HasDatabaseName("UQ_RefreshTokens_TokenHash");
            entity.HasIndex(token => new { token.UserId, token.ExpiresAt })
                .IncludeProperties(token => token.RevokedAt)
                .HasDatabaseName("IX_RefreshTokens_User_ExpiresAt");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_RefreshTokens_Users");
        });

        return modelBuilder;
    }
}
