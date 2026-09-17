using LedgerFlow.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Context.Configurations;

public static class UserTableConfiguration
{
    public const string TableName = "[dbo].[Users]";
    public const string ReadProjection = "Id, Name, Email, PasswordHash, Role, CreatedAt";

    public static ModelBuilder ConfigureUserTable(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", DatabaseConfiguration.Schema, table =>
                table.HasCheckConstraint("CK_Users_Role", "[Role] IN ('User', 'Admin')"));

            entity.HasKey(user => user.Id).HasName("PK_Users");
            entity.Property(user => user.Name).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(254).IsRequired();
            entity.Property(user => user.NormalizedEmail).HasMaxLength(254).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(user => user.CreatedAt).HasColumnType("datetimeoffset(0)");

            entity.HasIndex(user => user.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("UQ_Users_NormalizedEmail");
        });

        return modelBuilder;
    }
}
