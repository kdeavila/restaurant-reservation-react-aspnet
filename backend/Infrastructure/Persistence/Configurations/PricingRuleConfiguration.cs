using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Infrastructure.Persistence.Configurations;

public class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
{
    public void Configure(EntityTypeBuilder<PricingRule> builder)
    {
        builder.ToTable("PricingRules");
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.RuleName).IsRequired().HasMaxLength(100);

        builder.Property(pr => pr.RuleType).IsRequired().HasMaxLength(50);

        builder.Property(pr => pr.SurchargePercentage).HasColumnType("decimal(5,2)");

        builder.Property(pr => pr.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(pr => pr.CreatedAt).HasDefaultValueSql("NOW()");

        builder
            .HasOne(pr => pr.TableType)
            .WithMany(tt => tt.PricingRules)
            .HasForeignKey(pr => pr.TableTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
