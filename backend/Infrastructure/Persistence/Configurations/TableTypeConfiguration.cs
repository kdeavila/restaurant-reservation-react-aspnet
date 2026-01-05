using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Infrastructure.Persistence.Configurations;

public class TableTypeConfiguration : IEntityTypeConfiguration<TableType>
{
    public void Configure(EntityTypeBuilder<TableType> builder)
    {
        builder.ToTable("TableTypes");
        builder.HasKey(tt => tt.Id);

        builder.Property(tt => tt.Name).IsRequired().HasMaxLength(100);

        builder.HasIndex(tt => tt.Name).IsUnique();

        builder.Property(tt => tt.Description).HasMaxLength(500);

        builder.Property(tt => tt.BasePricePerHour).IsRequired().HasColumnType("decimal(18,2)");

        builder.Property(tt => tt.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(tt => tt.CreatedAt).HasDefaultValueSql("NOW()");
    }
}
