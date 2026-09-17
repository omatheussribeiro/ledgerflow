using LedgerFlow.Domain.Financial;
using LedgerFlow.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Context.Configurations;

public static class AccountTableConfiguration
{
    public const string TableName = "[dbo].[Accounts]";
    public const string ReadProjection = "a.Id, a.Name, a.Type, a.InitialBalance";

    public static ModelBuilder ConfigureAccountTable(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts", DatabaseConfiguration.Schema, table =>
                table.HasCheckConstraint("CK_Accounts_Type", "[Type] BETWEEN 1 AND 4"));

            entity.HasKey(account => account.Id).HasName("PK_Accounts");
            entity.Property(account => account.Name).HasMaxLength(100).IsRequired();
            entity.Property(account => account.Type).HasColumnType("tinyint");
            entity.Property(account => account.InitialBalance).HasPrecision(19, 2);
            entity.Property(account => account.IsActive).HasDefaultValue(true);
            entity.Property(account => account.CreatedAt).HasColumnType("datetimeoffset(0)");
            entity.Property(account => account.DeletedAt).HasColumnType("datetimeoffset(0)");
            entity.HasQueryFilter(account => account.DeletedAt == null);

            entity.HasIndex(account => new { account.UserId, account.Name })
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL")
                .HasDatabaseName("UQ_Accounts_User_Name");
            entity.HasIndex(account => new { account.UserId, account.DeletedAt })
                .IncludeProperties(account => new
                {
                    account.IsActive,
                    account.Name,
                    account.Type,
                    account.InitialBalance
                })
                .HasDatabaseName("IX_Accounts_User_DeletedAt");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(account => account.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Accounts_Users");
        });

        return modelBuilder;
    }
}
