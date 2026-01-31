using FluentAssertions;
using Moq;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Services;
using RestaurantReservation.Domain.Entities;
using Xunit;

namespace RestaurantReservation.API.Tests.Unit.Services;

public class PricingServiceTests
{
    private readonly Mock<ITableRepository> _tableRepo = new();
    private readonly Mock<ITableTypeRepository> _tableTypeRepo = new();
    private readonly Mock<IPricingRuleRepository> _pricingRuleRepo = new();

    private PricingService CreateSut() => new(_tableRepo.Object, _tableTypeRepo.Object, _pricingRuleRepo.Object);

    [Fact]
    public async Task CalculatePriceAsync_returns_base_price_when_no_rules()
    {
        var table = new Table { Id = 1, TableTypeId = 10 };
        var tableType = new TableType { Id = 10, BasePricePerHour = 25m };

        _tableRepo.Setup(r => r.GetByIdAsync(table.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(table);
        _tableTypeRepo.Setup(r => r.GetByIdAsync(tableType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tableType);
        _pricingRuleRepo.Setup(r => r.GetApplicableRulesAsync(table.TableTypeId, It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PricingRule>());

        var sut = CreateSut();
        var result = await sut.CalculatePriceAsync(table.Id, DateTime.UtcNow, TimeSpan.FromHours(12), TimeSpan.FromHours(14));

        result.IsFailure.Should().BeFalse();
        result.Value.BasePrice.Should().Be(50m);
        result.Value.TotalPrice.Should().Be(50m);
    }

    [Fact]
    public async Task CalculatePriceAsync_applies_surcharge_rules()
    {
        var table = new Table { Id = 1, TableTypeId = 10 };
        var tableType = new TableType { Id = 10, BasePricePerHour = 10m };
        var rule = new PricingRule { SurchargePercentage = 20m };

        _tableRepo.Setup(r => r.GetByIdAsync(table.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(table);
        _tableTypeRepo.Setup(r => r.GetByIdAsync(tableType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tableType);
        _pricingRuleRepo.Setup(r => r.GetApplicableRulesAsync(table.TableTypeId, It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PricingRule> { rule });

        var sut = CreateSut();
        var result = await sut.CalculatePriceAsync(table.Id, DateTime.UtcNow, TimeSpan.FromHours(18), TimeSpan.FromHours(20));

        result.IsFailure.Should().BeFalse();
        result.Value.BasePrice.Should().Be(20m);
        result.Value.TotalPrice.Should().Be(24m);
    }
}
