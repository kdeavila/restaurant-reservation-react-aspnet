using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Infrastructure.Persistence.Configurations;

public class PricingRuleDaysConfiguration : IEntityTypeConfiguration<PricingRuleDays>
{
    public void Configure(EntityTypeBuilder<PricingRuleDays> builder)
    {
        builder.ToTable("PricingRuleDays");
        builder.HasKey(prd => prd.Id);

        builder.Property(prd => prd.DayOfWeek)
            .IsRequired();

        builder.HasOne(prd => prd.PricingRule)
            .WithMany(pd => pd.PricingRuleDays)
            .HasForeignKey(prd => prd.PricingRuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}