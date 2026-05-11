using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Represents a subscription (utility/service) tied to a customer.
/// A subscription can generate multiple payments over time.
/// </summary>
public class Subscription : BaseEntity
{
    public Guid CustomerId { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
    public string ServiceProviderName { get; set; } = string.Empty;
    public string SubscriptionNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
