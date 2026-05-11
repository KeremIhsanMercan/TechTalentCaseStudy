using Application.DTOs.Payments;
using Application.DTOs.Subscriptions;
using Application.DTOs.Summary;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Application service for dashboard summary and reporting views.
/// Provides aggregated data about active subscriptions, unpaid periods, and payment history.
/// </summary>
public class SummaryAppService : ISummaryAppService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<SummaryAppService> _logger;

    public SummaryAppService(
        ISubscriptionRepository subscriptionRepository,
        IPaymentRepository paymentRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<SummaryAppService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<List<SubscriptionDto>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync(cancellationToken);
        return subscriptions.Select(s => new SubscriptionDto
        {
            Id = s.Id,
            CustomerId = s.CustomerId,
            CustomerFullName = s.Customer != null
                ? $"{s.Customer.FirstName} {s.Customer.LastName}"
                : string.Empty,
            SubscriptionType = s.SubscriptionType,
            SubscriptionTypeName = s.SubscriptionType.ToString(),
            ServiceProviderName = s.ServiceProviderName,
            SubscriptionNumber = s.SubscriptionNumber,
            IsActive = s.IsActive,
            CreatedDate = s.CreatedDate
        }).ToList();
    }

    public async Task<List<UnpaidSubscriptionDto>> GetUnpaidSubscriptionsForCurrentMonthAsync(
        CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var currentPeriod = $"{now.Year}_{now.Month:D2}";
        var unpaid = new List<UnpaidSubscriptionDto>();

        var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync(cancellationToken);

        foreach (var subscription in activeSubscriptions)
        {
            var isPaid = await _paymentRepository.ExistsSuccessfulPaymentAsync(
                subscription.Id, currentPeriod, cancellationToken);

            if (!isPaid)
            {
                unpaid.Add(new UnpaidSubscriptionDto
                {
                    SubscriptionId = subscription.Id,
                    CustomerId = subscription.CustomerId,
                    CustomerFullName = subscription.Customer != null
                        ? $"{subscription.Customer.FirstName} {subscription.Customer.LastName}"
                        : string.Empty,
                    SubscriptionTypeName = subscription.SubscriptionType.ToString(),
                    ServiceProviderName = subscription.ServiceProviderName,
                    SubscriptionNumber = subscription.SubscriptionNumber,
                    CurrentPeriod = currentPeriod
                });
            }
        }

        _logger.LogInformation("Unpaid subscriptions for period {Period}: {Count}", currentPeriod, unpaid.Count);
        return unpaid;
    }

    public async Task<List<SubscriptionPaymentSummaryDto>> GetSubscriptionPaymentHistoryAsync(
        Guid? subscriptionId = null, CancellationToken cancellationToken = default)
    {
        var result = new List<SubscriptionPaymentSummaryDto>();

        List<Domain.Entities.Subscription> subscriptions;

        if (subscriptionId.HasValue)
        {
            var sub = await _subscriptionRepository.GetByIdWithCustomerAsync(subscriptionId.Value, cancellationToken);
            subscriptions = sub != null ? new List<Domain.Entities.Subscription> { sub } : new();
        }
        else
        {
            subscriptions = await _subscriptionRepository.GetAllAsync(cancellationToken);
        }

        foreach (var subscription in subscriptions)
        {
            var payments = await _paymentRepository.GetBySubscriptionIdAsync(subscription.Id, cancellationToken);

            var summary = new SubscriptionPaymentSummaryDto
            {
                SubscriptionId = subscription.Id,
                SubscriptionTypeName = subscription.SubscriptionType.ToString(),
                ServiceProviderName = subscription.ServiceProviderName,
                SubscriptionNumber = subscription.SubscriptionNumber,
                IsActive = subscription.IsActive,
                Payments = payments.Select(p => new PaymentHistoryDto
                {
                    Id = p.Id,
                    SubscriptionId = p.SubscriptionId,
                    SubscriptionNumber = subscription.SubscriptionNumber,
                    ServiceProviderName = subscription.ServiceProviderName,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    Period = p.Period,
                    IsSuccessful = p.IsSuccessful,
                    CreatedDate = p.CreatedDate
                }).OrderByDescending(p => p.PaymentDate).ToList()
            };

            result.Add(summary);
        }

        return result;
    }
}
