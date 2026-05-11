using Domain.Enums;

namespace Application.DTOs.Subscriptions;

/// <summary>
/// Read DTO for subscription data. Strictly separated from the Domain Entity.
/// </summary>
public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerFullName { get; set; } = string.Empty;
    public SubscriptionType SubscriptionType { get; set; }
    public string SubscriptionTypeName { get; set; } = string.Empty;
    public string ServiceProviderName { get; set; } = string.Empty;
    public string SubscriptionNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal CurrentDebtAmount { get; set; }
    public DateTime NextDueDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
