using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Infrastructure.Persistence.Configurations;

public class PricingRuleDaysConfiguration : IEntityTypeConfiguration<PricingRuleDays>
{
    public void Configure(EntityTypeBuilder<PricingRuleDays> builder)
    {
        builder.ToTable("PricingRuleDays");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.PricingRuleId)
            .IsRequired();
        
        builder.Property(e => e.DayOfWeek)
            .IsRequired();

        // Navigation: PricingRuleDays (many) -> PricingRule (1)
        builder.HasOne(prd => prd.PricingRule)
            .WithMany(pr => pr.PricingRuleDays)
            .HasForeignKey(prd => prd.PricingRuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}