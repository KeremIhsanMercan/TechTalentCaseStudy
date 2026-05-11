using Application.DTOs.Payments;
using Application.DTOs.Subscriptions;
using Application.DTOs.Summary;

namespace Application.Interfaces;

/// <summary>
/// Application service contract for dashboard summary/reporting views.
/// </summary>
public interface ISummaryAppService
{
    /// <summary>
    /// Returns all active subscriptions.
    /// </summary>
    Task<List<SubscriptionDto>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns subscriptions that have no successful payment for the current billing period.
    /// </summary>
    Task<List<UnpaidSubscriptionDto>> GetUnpaidSubscriptionsForCurrentMonthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns payment history grouped by subscription.
    /// </summary>
    Task<List<SubscriptionPaymentSummaryDto>> GetSubscriptionPaymentHistoryAsync(Guid? subscriptionId = null, CancellationToken cancellationToken = default);
}
