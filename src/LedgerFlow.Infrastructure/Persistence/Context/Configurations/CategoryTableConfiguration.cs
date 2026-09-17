using LedgerFlow.Domain.Financial;
using LedgerFlow.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Context.Configurations;

public static class CategoryTableConfiguration
{
    public const string TableName = "[dbo].[Categories]";
    public const string ReadProjection = "c.Id, c.Name, c.Type, c.Color";

    public static ModelBuilder ConfigureCategoryTable(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories", DatabaseConfiguration.Schema, table =>
            {
                table.HasCheckConstraint("CK_Categories_Type", "[Type] IN (1, 2)");
                table.HasCheckConstraint(
                    "CK_Categories_Color",
                    "[Color] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'");
            });

            entity.HasKey(category => category.Id).HasName("PK_Categories");
            entity.Property(category => category.Name).HasMaxLength(80).IsRequired();
            entity.Property(category => category.Type).HasColumnType("tinyint");
            entity.Property(category => category.Color)
                .HasColumnType("char(7)")
                .IsUnicode(false)
                .IsFixedLength()
                .IsRequired();
            entity.Property(category => category.CreatedAt).HasColumnType("datetimeoffset(0)");
            entity.Property(category => category.DeletedAt).HasColumnType("datetimeoffset(0)");
            entity.HasQueryFilter(category => category.DeletedAt == null);

            entity.HasIndex(category => new { category.UserId, category.Type, category.Name })
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL")
                .HasDatabaseName("UQ_Categories_User_Type_Name");
            entity.HasIndex(category => new { category.UserId, category.DeletedAt })
                .IncludeProperties(category => new { category.Name, category.Type, category.Color })
                .HasDatabaseName("IX_Categories_User_DeletedAt");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(category => category.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Categories_Users");
        });

        return modelBuilder;
    }
}
