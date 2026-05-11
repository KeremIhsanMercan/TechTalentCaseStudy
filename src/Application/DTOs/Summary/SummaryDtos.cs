using Application.DTOs.Payments;
using Application.DTOs.Subscriptions;

namespace Application.DTOs.Summary;

/// <summary>
/// DTO for the subscription payment history view, grouping payments by subscription.
/// </summary>
public class SubscriptionPaymentSummaryDto
{
    public Guid SubscriptionId { get; set; }
    public string SubscriptionTypeName { get; set; } = string.Empty;
    public string ServiceProviderName { get; set; } = string.Empty;
    public string SubscriptionNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<PaymentHistoryDto> Payments { get; set; } = new();
}

/// <summary>
/// DTO for subscriptions that are unpaid for the current billing period.
/// </summary>
public class UnpaidSubscriptionDto
{
    public Guid SubscriptionId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerFullName { get; set; } = string.Empty;
    public string SubscriptionTypeName { get; set; } = string.Empty;
    public string ServiceProviderName { get; set; } = string.Empty;
    public string SubscriptionNumber { get; set; } = string.Empty;
    public string CurrentPeriod { get; set; } = string.Empty;
}
