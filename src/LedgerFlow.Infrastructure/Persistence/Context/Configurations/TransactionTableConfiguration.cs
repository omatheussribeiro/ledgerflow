using LedgerFlow.Domain.Financial;
using LedgerFlow.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Context.Configurations;

public static class TransactionTableConfiguration
{
    public const string TableName = "[dbo].[Transactions]";
    public const string ReadProjection =
        "t.Id, t.AccountId, a.Name AS AccountName, t.CategoryId, c.Name AS CategoryName, " +
        "c.Color AS CategoryColor, t.Type, t.Description, t.Amount, t.OccurredOn, t.Status, t.Notes";

    public static ModelBuilder ConfigureTransactionTable(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.ToTable("Transactions", DatabaseConfiguration.Schema, table =>
            {
                table.HasCheckConstraint("CK_Transactions_Type", "[Type] IN (1, 2)");
                table.HasCheckConstraint("CK_Transactions_Status", "[Status] BETWEEN 1 AND 3");
                table.HasCheckConstraint("CK_Transactions_Amount", "[Amount] > 0");
            });

            entity.HasKey(transaction => transaction.Id).HasName("PK_Transactions");
            entity.Property(transaction => transaction.Type).HasColumnType("tinyint");
            entity.Property(transaction => transaction.Description).HasMaxLength(160).IsRequired();
            entity.Property(transaction => transaction.Amount).HasPrecision(19, 2);
            entity.Property(transaction => transaction.OccurredOn).HasColumnType("date");
            entity.Property(transaction => transaction.Status).HasColumnType("tinyint");
            entity.Property(transaction => transaction.Notes).HasMaxLength(500);
            entity.Property<Guid?>("TransferGroupId");
            entity.Property<Guid?>("RecurrenceId");
            entity.Property(transaction => transaction.CreatedAt).HasColumnType("datetimeoffset(0)");
            entity.Property(transaction => transaction.DeletedAt).HasColumnType("datetimeoffset(0)");
            entity.HasQueryFilter(transaction => transaction.DeletedAt == null);

            entity.HasIndex(transaction => new { transaction.UserId, transaction.OccurredOn })
                .IsDescending(false, true)
                .IncludeProperties(transaction => new
                {
                    transaction.AccountId,
                    transaction.CategoryId,
                    transaction.Type,
                    transaction.Status,
                    transaction.Amount,
                    transaction.Description,
                    transaction.CreatedAt
                })
                .HasDatabaseName("IX_Transactions_User_OccurredOn");
            entity.HasIndex(transaction => new { transaction.AccountId, transaction.Status })
                .IncludeProperties(transaction => new { transaction.Type, transaction.Amount })
                .HasDatabaseName("IX_Transactions_Account_Status");
            entity.HasIndex(transaction => new { transaction.UserId, transaction.Type, transaction.OccurredOn })
                .IncludeProperties(transaction => new { transaction.CategoryId, transaction.Status, transaction.Amount })
                .HasDatabaseName("IX_Transactions_User_Type_OccurredOn");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(transaction => transaction.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Transactions_Users");
            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(transaction => transaction.AccountId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Transactions_Accounts");
            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(transaction => transaction.CategoryId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Transactions_Categories");
        });

        return modelBuilder;
    }
}
