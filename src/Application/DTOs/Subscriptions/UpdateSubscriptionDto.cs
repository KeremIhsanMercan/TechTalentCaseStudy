using Domain.Enums;

namespace Application.DTOs.Subscriptions;

/// <summary>
/// DTO for updating an existing subscription.
/// Only mutable business fields are exposed.
/// </summary>
public class UpdateSubscriptionDto
{
    public Guid Id { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
    public string ServiceProviderName { get; set; } = string.Empty;
    public string SubscriptionNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
