using Application.DTOs;
using Application.DTOs.Reminders;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Application.UnitTests.Services;

public class ReminderAppServiceTests
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDebtCheckingService _debtCheckingService;
    private readonly INotificationService _notificationService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ReminderAppService> _logger;
    private readonly IOptions<ReminderSettings> _reminderSettings;
    private readonly ReminderAppService _sut;

    private readonly DateTime _fixedNow = new DateTime(2024, 5, 1, 10, 0, 0, DateTimeKind.Utc);

    public ReminderAppServiceTests()
    {
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _debtCheckingService = Substitute.For<IDebtCheckingService>();
        _notificationService = Substitute.For<INotificationService>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _logger = Substitute.For<ILogger<ReminderAppService>>();
        
        var settings = new ReminderSettings { NotificationThresholdDays = 5 };
        _reminderSettings = Options.Create(settings);

        _dateTimeProvider.UtcNow.Returns(_fixedNow);

        _sut = new ReminderAppService(
            _subscriptionRepository,
            _paymentRepository,
            _debtCheckingService,
            _notificationService,
            _dateTimeProvider,
            _reminderSettings,
            _logger);
    }

    [Fact]
    public async Task GetPendingRemindersAsync_ShouldReturnReminders_WhenDueInThresholdAndUnpaid()
    {
        // Arrange
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            SubscriptionNumber = "SUB123",
            SubscriptionType = SubscriptionType.Elektrik,
            ServiceProviderName = "EnergyCorp",
            IsActive = true,
            CustomerId = Guid.NewGuid(),
            Customer = new Customer { FirstName = "John", LastName = "Doe", Email = "john@example.com" }
        };

        _subscriptionRepository.GetActiveSubscriptionsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { subscription });

        _paymentRepository.ExistsSuccessfulPaymentAsync(subscription.Id, "2024_05", Arg.Any<CancellationToken>())
            .Returns(false);

        _debtCheckingService.CheckDebtAsync(subscription.SubscriptionNumber, subscription.SubscriptionType.ToString(), Arg.Any<decimal>(), Arg.Any<DateTime>())
            .Returns(new DebtQueryResultDto
            {
                DebtAmount = 150.00m,
                DueDate = _fixedNow.AddDays(3),
                Period = "2024_05"
            });

        // Act
        var result = await _sut.GetPendingRemindersAsync(5);

        // Assert
        result.Should().HaveCount(1);
        result[0].SubscriptionNumber.Should().Be("SUB123");
        result[0].DaysUntilDue.Should().Be(3);
        result[0].DebtAmount.Should().Be(150.00m);
    }

    [Fact]
    public async Task GetPendingRemindersAsync_ShouldSkip_WhenAlreadyPaid()
    {
        // Arrange
        var subscription = new Subscription { Id = Guid.NewGuid(), IsActive = true };

        _subscriptionRepository.GetActiveSubscriptionsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { subscription });

        _paymentRepository.ExistsSuccessfulPaymentAsync(subscription.Id, "2024_05", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.GetPendingRemindersAsync(5);

        // Assert
        result.Should().BeEmpty();
        await _debtCheckingService.DidNotReceiveWithAnyArgs().CheckDebtAsync(default!, default!, default, default);
    }

    [Fact]
    public async Task GetPendingRemindersAsync_ShouldSkip_WhenDueBeyondThreshold()
    {
        // Arrange
        var subscription = new Subscription { Id = Guid.NewGuid(), IsActive = true };

        _subscriptionRepository.GetActiveSubscriptionsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { subscription });

        _paymentRepository.ExistsSuccessfulPaymentAsync(subscription.Id, "2024_05", Arg.Any<CancellationToken>())
            .Returns(false);

        _debtCheckingService.CheckDebtAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<DateTime>())
            .Returns(new DebtQueryResultDto
            {
                DebtAmount = 100m,
                DueDate = _fixedNow.AddDays(10), // Threshold is 5
                Period = "2024_05"
            });

        // Act
        var result = await _sut.GetPendingRemindersAsync(5);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingRemindersAsync_ShouldHandleDebtCheckFailureGracefully_AndReturnOtherReminders()
    {
        // Arrange
        var sub1 = new Subscription { Id = Guid.NewGuid(), SubscriptionNumber = "S1", IsActive = true };
        var sub2 = new Subscription { Id = Guid.NewGuid(), SubscriptionNumber = "S2", IsActive = true };

        _subscriptionRepository.GetActiveSubscriptionsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { sub1, sub2 });

        _paymentRepository.ExistsSuccessfulPaymentAsync(Arg.Any<Guid>(), "2024_05", Arg.Any<CancellationToken>())
            .Returns(false);

        // Sub 1 fails
        _debtCheckingService.CheckDebtAsync("S1", Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<DateTime>())
            .ThrowsAsync(new Exception("Network error"));

        // Sub 2 succeeds
        _debtCheckingService.CheckDebtAsync("S2", Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<DateTime>())
            .Returns(new DebtQueryResultDto { DebtAmount = 50m, DueDate = _fixedNow.AddDays(1), Period = "2024_05" });

        // Act
        var result = await _sut.GetPendingRemindersAsync(5);

        // Assert
        result.Should().HaveCount(1);
        result[0].SubscriptionNumber.Should().Be("S2");
    }

}
