using Domain.Enums;

namespace Application.DTOs.Subscriptions;

/// <summary>
/// DTO for creating a new subscription linked to an existing customer.
/// </summary>
public class CreateSubscriptionDto
{
    public Guid CustomerId { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
    public string ServiceProviderName { get; set; } = string.Empty;
    public string SubscriptionNumber { get; set; } = string.Empty;
}
